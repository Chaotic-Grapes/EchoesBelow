using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using System.Collections.Generic;




namespace EchoesBelow.Scripts.MarineSnowSystem;

[Component] public record struct MS_SpawnerComponent(bool start, float spawnIntervalinMilliseconds, int toSpawn, float timer,float decayTime, int spawnCountMin, int spawnCountMax, bool z_isGlobal);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class MS_Spawner : SystemBase
{
    int toSpawnMin;
    int toSpawnMax;
    float spawnInterval = 0;
    //toSpawn is formatted as 10000000, the 7 0's after 1 represent the different MS particles
    protected override void OnCreate()
    {
        //Log("System MS_Spawner initialized");
    }
    private bool OnStart(ref bool startBool, ulong objId, int spawnCountMax, int spawnCountMin)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo
        toSpawnMax = spawnCountMax;
        toSpawnMin = spawnCountMin;
        spawnInterval = Entity.FromId(World!, objId).GetComponent<MS_SpawnerComponent>().spawnIntervalinMilliseconds / 1000f;
        ref ShapeBox2D shapeBox = ref Entity.FromId(World!, objId).GetComponent<ShapeBox2D>();
        shapeBox.Filled = false;
        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        foreach(var gameObject in World!.Query<MS_SpawnerComponent>())
        {
            //A Pseudo Start function, called once per obj at runtime
            //This allows onStart to work
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start, gameObject.Entity.Id, gameObject.Component1.spawnCountMax, gameObject.Component1.spawnCountMin);

            gameObject.Component1.timer += Time.DeltaTime;

            Entity entity = Entity.FromId(World!, gameObject.Entity.Id);

            //If we exceed the spawn interval
            spawnInterval = gameObject.Component1.spawnIntervalinMilliseconds / 1000f;
            if(gameObject.Component1.timer >= spawnInterval)
            {
               
                //Reset the timer, prime it for the next interval
                gameObject.Component1.timer = 0;

                ShapeBox2D boundary = entity.GetComponent<ShapeBox2D>();
                LocalTransform transform = entity.GetComponent<LocalTransform>();
                //Set the x boundary min and max limits
                float xBoundaryMax = transform.Position.X + boundary.HalfExtents.X;
                float xBoundaryMin = transform.Position.X - boundary.HalfExtents.X;
                
                //Marine Snow selector
                string msID_raw = gameObject.Component1.toSpawn.ToString();
                msID_raw = msID_raw.Substring(1);
                char[] msID_array = msID_raw.ToCharArray();
                List<int> msID_selected = new List<int>();
                int id_iterator = 1;
                foreach(char c in msID_array)
                {
                    if(c=='1') msID_selected.Add(id_iterator);
                    id_iterator++;
                }

                int msID = msID_selected[(GMath.Random(0, msID_selected.Count-1))];

                //How many to spawn
                int spawnCount = GMath.Random(toSpawnMin, toSpawnMax);
                //Spawner
                for (int i = 0; i < spawnCount; i++)
                {
                    //Generate new spawn Coordinate
                    float xValue = GMath.Random(xBoundaryMin, xBoundaryMax);
                    //offset by player transform for now!
                    Vector3 spawnPos;


                    if (gameObject.Component1.z_isGlobal)
                    {
                        spawnPos = new Vector3(xValue + Player.instance.currentPos.X, transform.Position.Y + Player.instance.currentPos.Y, 0);
                    }
                    else
                    {
                        spawnPos = new Vector3(xValue , transform.Position.Y , 0);
                    }
                    if (MS_Manager.instance.TakeFromPool(msID, spawnPos, new Vector2(GMath.Random(0.5f, 2f), GMath.Random(0.5f, 2f)), gameObject.Component1.decayTime, false) == MS_Manager.instance.emptyId) continue;
                }
            }
        }
    }

}
