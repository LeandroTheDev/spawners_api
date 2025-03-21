using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace SpawnersAPI;

public class Spawner : BlockEntity
{
    static public event Action<Entity> OnSpawnerSpawn;
    private List<long> entitiesAlive = [];
    private List<string> entitiesToSpawn = ["game:drifter-normal"];
    private int progressSpawn = 0;
    private long progressTickID = 0;
    private string spawnerID = "";
    #region behaviours
    private bool torchWillDisableSpawn = false;
    private bool spawnOnlyInGround = false;
    private bool spawnOnlyWith2Heights = false;
    private bool droppable = false;
    private bool freezeOnAllEntitiesSpawned = false;
    private double healthAdditional = 1.0;
    private double damageAdditional = 1.0;
    private int lightLevel1 = 4;
    private int lightLevel2 = 7;
    private int lightLevel3 = 9;
    private int lightLevel4 = 13;
    private int maxSpawnedEntities = 20;
    private int maxEntitiesSpawnAtOnce = 4;
    private int xSpawnMaxDistance = 4;
    private int ySpawnMaxDistance = 2;
    private int zSpawnMaxDistance = 4;
    private int xPlayerDistanceToSpawn = 16;
    private int yPlayerDistanceToSpawn = 16;
    private int zPlayerDistanceToSpawn = 16;
    private int maxChancesToFindAValidBlockToSpawn = 30;
    private JArray spawnerDrops = [];
    private bool extendedLogs = false;
    #endregion

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        // Clients does not need to register a game tick listener
        if (api.Side != EnumAppSide.Client)
            progressTickID = RegisterGameTickListener(OnTickRate, 2000, 0);

        #region config-load
        spawnerID = Block.Code.ToString().Replace("spawnersapi:spawner-", "");
        try
        {
            Dictionary<string, object> baseConfigs = Api.Assets.Get(new AssetLocation($"spawnersapi:config/{spawnerID}.json")).ToObject<Dictionary<string, object>>();
            { //torchWillDisableSpawn
                if (baseConfigs.TryGetValue("torchWillDisableSpawn", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: torchWillDisableSpawn is null");
                    else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: torchWillDisableSpawn is not boolean is {value.GetType()}");
                    else torchWillDisableSpawn = (bool)value;
                else Debug.Log("CONFIGURATION ERROR: torchWillDisableSpawn not set");
            }
            { //spawnOnlyInGround
                if (baseConfigs.TryGetValue("spawnOnlyInGround", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: spawnOnlyInGround is null");
                    else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: spawnOnlyInGround is not boolean is {value.GetType()}");
                    else spawnOnlyInGround = (bool)value;
                else Debug.Log("CONFIGURATION ERROR: spawnOnlyInGround not set");
            }
            { //spawnOnlyWith2Heights
                if (baseConfigs.TryGetValue("spawnOnlyWith2Heights", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: spawnOnlyWith2Heights is null");
                    else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: spawnOnlyWith2Heights is not boolean is {value.GetType()}");
                    else spawnOnlyWith2Heights = (bool)value;
                else Debug.Log("CONFIGURATION ERROR: spawnOnlyWith2Heights not set");
            }
            { //droppable
                if (baseConfigs.TryGetValue("droppable", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: droppable is null");
                    else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: droppable is not boolean is {value.GetType()}");
                    else droppable = (bool)value;
                else Debug.Log("CONFIGURATION ERROR: droppable not set");
            }
            { //freezeOnAllEntitiesSpawned
                if (baseConfigs.TryGetValue("freezeOnAllEntitiesSpawned", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: freezeOnAllEntitiesSpawned is null");
                    else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: freezeOnAllEntitiesSpawned is not boolean is {value.GetType()}");
                    else freezeOnAllEntitiesSpawned = (bool)value;
                else Debug.Log("CONFIGURATION ERROR: freezeOnAllEntitiesSpawned not set");
            }
            { //healthAdditional
                if (baseConfigs.TryGetValue("healthAdditional", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: healthAdditional is null");
                    else if (value is not double) Debug.Log($"CONFIGURATION ERROR: healthAdditional is not double is {value.GetType()}");
                    else healthAdditional = (double)value;
                else Debug.Log("CONFIGURATION ERROR: healthAdditional not set");
            }
            { //damageAdditional
                if (baseConfigs.TryGetValue("damageAdditional", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: damageAdditional is null");
                    else if (value is not double) Debug.Log($"CONFIGURATION ERROR: damageAdditional is not double is {value.GetType()}");
                    else damageAdditional = (double)value;
                else Debug.Log("CONFIGURATION ERROR: damageAdditional not set");
            }
            { //entitiesToSpawn
                if (baseConfigs.TryGetValue("entitiesToSpawn", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: entitiesToSpawn is null");
                    else if (value is not JArray) Debug.Log($"CONFIGURATION ERROR: entitiesToSpawn is not List is {value.GetType()}");
                    else entitiesToSpawn = (value as JArray).ToObject<List<string>>();
                else Debug.Log("CONFIGURATION ERROR: entitiesToSpawn not set");
            }
            { //lightLevel1
                if (baseConfigs.TryGetValue("lightLevel1", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: lightLevel1 is null");
                    else if (value is not long) Debug.Log($"CONFIGURATION ERROR: lightLevel1 is not int is {value.GetType()}");
                    else lightLevel1 = (int)(long)value;
                else Debug.Log("CONFIGURATION ERROR: lightLevel1 not set");
            }
            { //lightLevel2
                if (baseConfigs.TryGetValue("lightLevel2", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: lightLevel2 is null");
                    else if (value is not long) Debug.Log($"CONFIGURATION ERROR: lightLevel2 is not int is {value.GetType()}");
                    else lightLevel2 = (int)(long)value;
                else Debug.Log("CONFIGURATION ERROR: lightLevel2 not set");
            }
            { //lightLevel3
                if (baseConfigs.TryGetValue("lightLevel3", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: lightLevel3 is null");
                    else if (value is not long) Debug.Log($"CONFIGURATION ERROR: lightLevel3 is not int is {value.GetType()}");
                    else lightLevel3 = (int)(long)value;
                else Debug.Log("CONFIGURATION ERROR: lightLevel3 not set");
            }
            { //lightLevel4
                if (baseConfigs.TryGetValue("lightLevel4", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: lightLevel4 is null");
                    else if (value is not long) Debug.Log($"CONFIGURATION ERROR: lightLevel4 is not int is {value.GetType()}");
                    else lightLevel4 = (int)(long)value;
                else Debug.Log("CONFIGURATION ERROR: lightLevel4 not set");
            }
            { //maxSpawnedEntities
                if (baseConfigs.TryGetValue("maxSpawnedEntities", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: maxSpawnedEntities is null");
                    else if (value is not long) Debug.Log($"CONFIGURATION ERROR: maxSpawnedEntities is not int is {value.GetType()}");
                    else maxSpawnedEntities = (int)(long)value;
                else Debug.Log("CONFIGURATION ERROR: maxSpawnedEntities not set");
            }
            { //maxSpawnedEntities
                if (baseConfigs.TryGetValue("maxEntitiesSpawnAtOnce", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: maxEntitiesSpawnAtOnce is null");
                    else if (value is not long) Debug.Log($"CONFIGURATION ERROR: maxEntitiesSpawnAtOnce is not int is {value.GetType()}");
                    else maxEntitiesSpawnAtOnce = (int)(long)value;
                else Debug.Log("CONFIGURATION ERROR: maxEntitiesSpawnAtOnce not set");
            }
            { //xSpawnMaxDistance
                if (baseConfigs.TryGetValue("xSpawnMaxDistance", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: xSpawnMaxDistance is null");
                    else if (value is not long) Debug.Log($"CONFIGURATION ERROR: xSpawnMaxDistance is not int is {value.GetType()}");
                    else xSpawnMaxDistance = (int)(long)value;
                else Debug.Log("CONFIGURATION ERROR: xSpawnMaxDistance not set");
            }
            { //ySpawnMaxDistance
                if (baseConfigs.TryGetValue("ySpawnMaxDistance", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: ySpawnMaxDistance is null");
                    else if (value is not long) Debug.Log($"CONFIGURATION ERROR: ySpawnMaxDistance is not int is {value.GetType()}");
                    else ySpawnMaxDistance = (int)(long)value;
                else Debug.Log("CONFIGURATION ERROR: ySpawnMaxDistance not set");
            }
            { //zSpawnMaxDistance
                if (baseConfigs.TryGetValue("zSpawnMaxDistance", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: zSpawnMaxDistance is null");
                    else if (value is not long) Debug.Log($"CONFIGURATION ERROR: zSpawnMaxDistance is not int is {value.GetType()}");
                    else zSpawnMaxDistance = (int)(long)value;
                else Debug.Log("CONFIGURATION ERROR: zSpawnMaxDistance not set");
            }
            { //xPlayerDistanceToSpawn
                if (baseConfigs.TryGetValue("xPlayerDistanceToSpawn", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: xPlayerDistanceToSpawn is null");
                    else if (value is not long) Debug.Log($"CONFIGURATION ERROR: xPlayerDistanceToSpawn is not int is {value.GetType()}");
                    else xPlayerDistanceToSpawn = (int)(long)value;
                else Debug.Log("CONFIGURATION ERROR: xPlayerDistanceToSpawn not set");
            }
            { //yPlayerDistanceToSpawn
                if (baseConfigs.TryGetValue("yPlayerDistanceToSpawn", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: yPlayerDistanceToSpawn is null");
                    else if (value is not long) Debug.Log($"CONFIGURATION ERROR: yPlayerDistanceToSpawn is not int is {value.GetType()}");
                    else yPlayerDistanceToSpawn = (int)(long)value;
                else Debug.Log("CONFIGURATION ERROR: yPlayerDistanceToSpawn not set");
            }
            { //zPlayerDistanceToSpawn
                if (baseConfigs.TryGetValue("zPlayerDistanceToSpawn", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: zPlayerDistanceToSpawn is null");
                    else if (value is not long) Debug.Log($"CONFIGURATION ERROR: zPlayerDistanceToSpawn is not int is {value.GetType()}");
                    else zPlayerDistanceToSpawn = (int)(long)value;
                else Debug.Log("CONFIGURATION ERROR: zPlayerDistanceToSpawn not set");
            }
            { //maxChancesToFindAValidBlockToSpawn
                if (baseConfigs.TryGetValue("maxChancesToFindAValidBlockToSpawn", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: maxChancesToFindAValidBlockToSpawn is null");
                    else if (value is not long) Debug.Log($"CONFIGURATION ERROR: maxChancesToFindAValidBlockToSpawn is not int is {value.GetType()}");
                    else maxChancesToFindAValidBlockToSpawn = (int)(long)value;
                else Debug.Log("CONFIGURATION ERROR: maxChancesToFindAValidBlockToSpawn not set");
            }
            { //spawnerDrops
                if (baseConfigs.TryGetValue("spawnerDrops", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: spawnerDrops is null");
                    else if (value is not JArray) Debug.Log($"CONFIGURATION ERROR: spawnerDrops is not List is {value.GetType()}");
                    else spawnerDrops = value as JArray;
                else Debug.Log("CONFIGURATION ERROR: spawnerDrops not set");
            }
            { //extendedLogs
                if (baseConfigs.TryGetValue("extendedLogs", out object value))
                    if (value is null) Debug.Log("CONFIGURATION ERROR: extendedLogs is null");
                    else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: extendedLogs is not boolean is {value.GetType()}");
                    else extendedLogs = (bool)value;
                else Debug.Log("CONFIGURATION ERROR: extendedLogs not set");
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"ERROR: {ex.Message}");
        }
        if (!droppable) Block.Drops = [];
        #endregion
    }

    private void OnTickRate(float obj)
    {
        // Checking entities alive
        for (int i = entitiesAlive.Count - 1; i >= 0; i--)
        {
            long id = entitiesAlive[i];
            Entity entity = Api.World.GetEntityById(id);
            if (entity is not null)
            {
                if (!entity.Alive) entitiesAlive.RemoveAt(i);
            }
            else entitiesAlive.RemoveAt(i);
        }
        // Getting the block
        Block spawnerBlock = Api.World.BlockAccessor.GetBlock(Pos);
        // Check the freeze on all entities spawned condition
        if (freezeOnAllEntitiesSpawned && entitiesAlive.Count >= maxSpawnedEntities)
        {
            if (extendedLogs)
                Debug.Log($"Progress is freezed, entities alive: {entitiesAlive.Count}");
            return;
        };
        // Check block existance
        if (spawnerBlock == null || !spawnerBlock.Code.ToString().Contains("spawnersapi:spawner"))
        {
            if (extendedLogs)
                Debug.Log($"{Block.Code} doesn't exist anymore removing tickrate");
            if (progressTickID != 0)
                UnregisterGameTickListener(progressTickID);
            return;
        };

        if (extendedLogs) Debug.Log($"{Block.Code} Validation Process");
        #region check-near-torchs
        if (torchWillDisableSpawn)
        {
            { // North torch detection
                BlockPos newPosition = Pos.Copy();
                newPosition.X += 1;
                Block receivedBlock = Api.World.BlockAccessor.GetBlock(newPosition);
                if (extendedLogs) Debug.Log($"{Block.Code} North Block: {receivedBlock.Code}");
                if (receivedBlock.Code.ToString().Contains("game:torch-basic-lit")) return;
            }

            { // South torch detection
                BlockPos newPosition = Pos.Copy();
                newPosition.X -= 1;
                Block receivedBlock = Api.World.BlockAccessor.GetBlock(newPosition);
                if (extendedLogs) Debug.Log($"{Block.Code} South Block: {receivedBlock.Code}");
                if (receivedBlock.Code.ToString().Contains("game:torch-basic-lit")) return;
            }

            { // East torch detection
                BlockPos newPosition = Pos.Copy();
                newPosition.Z += 1;
                Block receivedBlock = Api.World.BlockAccessor.GetBlock(newPosition);
                if (extendedLogs) Debug.Log($"{Block.Code} East Block: {receivedBlock.Code}");
                if (receivedBlock.Code.ToString().Contains("game:torch-basic-lit")) return;
            }

            { // West torch detection
                BlockPos newPosition = Pos.Copy();
                newPosition.Z -= 1;
                Block receivedBlock = Api.World.BlockAccessor.GetBlock(newPosition);
                if (extendedLogs) Debug.Log($"{Block.Code} West Block: {receivedBlock.Code}");
                if (receivedBlock.Code.ToString().Contains("game:torch-basic-lit")) return;
            }

            { // On the ground torch detection, used when the spawner is flying
                BlockPos newPosition = Pos.Copy();
                newPosition.Y -= 1;
                Block receivedBlock = Api.World.BlockAccessor.GetBlock(newPosition);
                if (extendedLogs) Debug.Log($"{Block.Code} Ground Block: {receivedBlock.Code}");
                if (receivedBlock.Code.ToString().Contains("game:torch-basic-lit")) return;
            }

            { // On the top torch detection
                BlockPos newPosition = Pos.Copy();
                newPosition.Y += 1;
                Block receivedBlock = Api.World.BlockAccessor.GetBlock(newPosition);
                if (extendedLogs) Debug.Log($"{Block.Code} Top Block: {receivedBlock.Code}");
                if (receivedBlock.Code.ToString().Contains("game:torch-basic-lit")) return;
            }
        }
        if (extendedLogs) Debug.Log($"{Block.Code} check-near-torchs: OK, config: {torchWillDisableSpawn}");
        #endregion
        #region check-near-players
        IPlayer nearestPlayer = Api.World.NearestPlayer(Pos.X, Pos.Y, Pos.Z);
        if (nearestPlayer == null) return;
        else
        {
            // Getting positions
            double playerX = nearestPlayer.Entity.Pos.X;
            double playerY = nearestPlayer.Entity.Pos.Y;
            double playerZ = nearestPlayer.Entity.Pos.Z;
            double blockX = Pos.X;
            double blockY = Pos.Y;
            double blockZ = Pos.Z;

            // X Calculation
            double xDistance;
            if (playerX < blockX) xDistance = blockX - playerX;
            else xDistance = playerX - blockX;

            // Y Calcuation
            double yDistance;
            if (playerY < blockY) yDistance = blockY - playerY;
            else yDistance = playerY - blockY;

            // Z Calculation
            double zDistance;
            if (playerZ < blockZ) zDistance = blockZ - playerZ;
            else zDistance = playerY - blockY;

            // Distance check
            if (xDistance > xPlayerDistanceToSpawn || yDistance > yPlayerDistanceToSpawn || zDistance > zPlayerDistanceToSpawn)
                return;

            if (extendedLogs) Debug.Log($"{Block.Code} check-near-players: OK, XYZ: {(int)xDistance}, {(int)yDistance}, {(int)zDistance}");
        }
        #endregion
        #region check-max-entities
        // Getting entities that is no longer alive
        List<long> entitiesToRemove = [];
        foreach (long entityId in entitiesAlive)
        {
            Entity entity = Api.World.GetEntityById(entityId);
            // Check the existance in the world
            if (entity == null)
            {
                entitiesToRemove.Add(entityId);
                continue;
            }
            // Check if is dead
            if (!entity.Alive)
            {
                entitiesToRemove.Add(entityId);
                continue;
            }
        }
        // Removing entities
        foreach (long entityId in entitiesToRemove) entitiesAlive.Remove(entityId);
        // Disabling spawner if the max entities reached
        if (entitiesAlive.Count >= maxSpawnedEntities) return;
        if (extendedLogs) Debug.Log($"{Block.Code} check-max-entities OK, quantity: {entitiesAlive.Count}");
        #endregion

        // Progress calculation based on the light level of the block
        int lightLevel = Api.World.BlockAccessor.GetLightLevel(Pos, EnumLightLevelType.MaxLight);
        if (lightLevel <= lightLevel1) progressSpawn += 10;
        else if (lightLevel <= lightLevel2) progressSpawn += 5;
        else if (lightLevel <= lightLevel3) progressSpawn += 3;
        else if (lightLevel < lightLevel4) progressSpawn += 2;
        else progressSpawn = 0;
        if (extendedLogs) Debug.Log($"{Block.Code} progress: {progressSpawn}, lightLevel: {lightLevel}");

        // Check final progress
        if (progressSpawn >= 100)
        {
            SpawnEntities();
            progressSpawn = 0;
        }
    }

    private void SpawnEntities()
    {
        if (extendedLogs) Debug.Log($"Entity Spawning...");
        float minX = Pos.X - xSpawnMaxDistance;
        float minY = Pos.Y - ySpawnMaxDistance;
        float minZ = Pos.Z - zSpawnMaxDistance;
        float maxX = Pos.X + xSpawnMaxDistance;
        float maxY = Pos.Y + ySpawnMaxDistance;
        float maxZ = Pos.Z + zSpawnMaxDistance;
        int maxChances = maxChancesToFindAValidBlockToSpawn;
        byte entitiesSpawned = 0;
        Random random = new();
        for (int i = 0; i < maxChances; i++)
        {
            i++;
            // Get a random block position
            int spawnX = random.Next((int)minX, (int)maxX);
            int spawnY = random.Next((int)minY, (int)maxY);
            int spawnZ = random.Next((int)minZ, (int)maxZ);
            // Get the block with the random position
            BlockPos spawnBlockPos = new(spawnX, spawnY, spawnZ, 0);
            Block spawnBlock = Api.World.BlockAccessor.GetBlock(spawnBlockPos);
            // Check if block is free
            if (spawnBlock.Code.ToString() == "game:air")
            {
                // Checking if the upper of the spawner is air
                if (spawnOnlyWith2Heights)
                {
                    BlockPos upperPosition = spawnBlockPos;
                    upperPosition.Y += 1;
                    Block upperBlock = Api.World.BlockAccessor.GetBlock(upperPosition);
                    if (upperBlock.Code.ToString() != "game:air") continue;
                }

                // Checking if the ground of the entity spawning is not air
                if (spawnOnlyInGround)
                {
                    BlockPos downPosition = spawnBlockPos;
                    downPosition.Y -= 1;
                    Block downBlock = Api.World.BlockAccessor.GetBlock(downPosition);
                    if (downBlock.Code.ToString() == "game:air") continue;
                }


                entitiesSpawned++;

                // Getting Entity info
                int selectedEntity = random.Next(0, entitiesToSpawn.Count); // Select a random entity
                EntityProperties type = Api.World.GetEntityType(new AssetLocation(entitiesToSpawn[selectedEntity]));
                // Check if entity exist
                if (type == null)
                {
                    Debug.Log($"ERROR: the entity from {Block.Code} is null: {entitiesToSpawn[selectedEntity]}");
                    break;
                }

                if (extendedLogs) Debug.Log($"valid position to spawn, entity spawning: {entitiesToSpawn[selectedEntity]}");

                // Instanciating
                Entity entity = Api.World.ClassRegistry.CreateEntity(type);
                // Setting the variable to be increased the damage
                entity.Attributes.SetDouble("SpawnersAPIDamageIncrease", damageAdditional);
                entity.Attributes.SetDouble("SpawnersAPIHealthIncrease", healthAdditional);
                entity.ServerPos.X = spawnX + 0.5;
                entity.ServerPos.Y = spawnY;
                entity.ServerPos.Z = spawnZ + 0.5;
                entity.Pos.SetPos(entity.ServerPos);
                entity.Attributes.SetBool("SpawnersAPI_Is_From_Spawner", true);
                // Spawning
                Api.World.SpawnEntity(entity);
                OnSpawnerSpawn?.Invoke(entity);
                entitiesAlive.Add(entity.EntityId);
                UpdateEntityStatus(entity);

                // Limit of maxSpawnedEntities once
                if (entitiesSpawned >= maxEntitiesSpawnAtOnce || entitiesAlive.Count >= maxSpawnedEntities) break;
            }
        }
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        tree.SetInt("progressSpawn", progressSpawn);
        tree.SetString("entitiesAlive", string.Join(",", entitiesAlive));
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        base.FromTreeAttributes(tree, worldForResolving);
        // Load progress
        progressSpawn = tree.GetInt("progressSpawn");

        // Load spawned entities
        string entitiesSaved = tree.GetString("entitiesAlive");
        if (entitiesSaved != null && entitiesSaved != "")
            entitiesAlive = entitiesSaved.Split(",").Select(long.Parse).ToList();
    }

    public override void OnBlockUnloaded()
    {
        base.OnBlockUnloaded();
        if (progressTickID != 0)
            UnregisterGameTickListener(progressTickID);
    }

    public override void OnBlockBroken(IPlayer byPlayer = null)
    {
        base.OnBlockBroken(byPlayer);
        // Particles when breaking
        SimpleParticleProperties props = new(15f, 22f, ColorUtil.ToRgba(150, 0, 0, 0), new Vec3d(Pos.X, Pos.Y, Pos.Z), new Vec3d(Pos.X + 1, Pos.Y + 1, Pos.Z + 1), new Vec3f(-0.2f, -0.1f, -0.2f), new Vec3f(0.2f, 0.2f, 0.2f), 1.5f, 0f, 0.5f, 1f, EnumParticleModel.Quad)
        {
            OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -200f),
            SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, 2f)
        };
        Api.World.SpawnParticles(props);
        SimpleParticleProperties spiders = new(8f, 16f, ColorUtil.ToRgba(255, 50, 50, 50), new Vec3d(Pos.X, Pos.Y, Pos.Z), new Vec3d(Pos.X + 1, Pos.Y + 1, Pos.Z + 1), new Vec3f(-2f, -0.3f, -2f), new Vec3f(2f, 1f, 2f), 1f, 0.5f, 0.5f, 1.5f);
        Api.World.SpawnParticles(spiders);

        // Loot drops
        foreach (ItemStack itemStack in GetItemDrops())
            Api.World.SpawnItemEntity(itemStack, Pos.ToVec3d());

    }

    private List<ItemStack> GetItemDrops()
    {
        if (extendedLogs) Debug.Log("starting loot drop calculation");
        Random random = new();
        JArray drops = null;
        { // Getting the drops by chance
            // This is used to increase performance
            // when the chance is too low and cannot get any item
            // we will increase all chance drop rates to finally find one
            int chanceIncreaser = 0;
            while (true)
            {
                // Swipe all drop lists
                foreach (JObject drop in spawnerDrops.Cast<JObject>())
                {
                    int chance = (int)drop["chance"] + chanceIncreaser;
                    if (chance >= random.Next(0, 100))
                    {
                        // Adding the drops to the drops table
                        drops = (JArray)drop["codes"];
                        break;
                    }
                }
                // Checking if we finded the drop
                if (drops != null) break;
                chanceIncreaser += 10;
            }
        }

        List<ItemStack> items = [];
        // Swiping every drop from the drops array
        foreach (JObject drop in drops.Cast<JObject>())
        {
            int chance = (int)drop["chance"];
            if (random.Next(0, 100) <= chance)
            {
                string itemCode = (string)drop["code"];
                AssetLocation code = new(itemCode);
                ItemStack item;

                // Item
                try
                {
                    item = new(Api.World.GetItem(code))
                    {
                        StackSize = random.Next((int)drop["minQuantity"], (int)drop["maxQuantity"] + 1)
                    };
                    items.Add(item);
                    continue;
                }
                catch (Exception) { }

                // Block
                try
                {
                    item = new(Api.World.GetBlock(code))
                    {
                        StackSize = random.Next((int)drop["minQuantity"], (int)drop["maxQuantity"] + 1)
                    };
                    items.Add(item);
                    continue;
                }
                catch (Exception) { }

                // Invalid
                Debug.Log($"ERROR: Cannot retrieve spawner drop because {itemCode} does not exist");
            }
        }
        return items;
    }

    private void UpdateEntityStatus(Entity entity)
    {
        // Changing Health Stats
        EntityBehaviorHealth entityLifeStats = entity.GetBehavior<EntityBehaviorHealth>();
        entityLifeStats.BaseMaxHealth += (float)(entityLifeStats.BaseMaxHealth * healthAdditional);
        entityLifeStats.Health += (float)(entityLifeStats.Health * healthAdditional);

        // Looking for the damage? is on Overwrite.cs
    }
}