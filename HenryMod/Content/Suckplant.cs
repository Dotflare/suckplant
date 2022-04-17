using BepInEx.Configuration;
using SuckplantMod.Modules.Characters;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using MonoMod.RuntimeDetour;
using R2API;

using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;
using RoR2.Navigation;
using RoR2.Orbs;
using RoR2.Skills;
using RoR2.UI;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine.UI;


namespace SuckplantMod.Modules.Survivors
{
    internal class Suckplant : SurvivorBase
    {
        public override string bodyName => "Suckplant";

        internal static int spawnCost = 1;
        internal static int minimumStageCount = 0;

        internal static bool enemyEnabled = true; 
        internal static bool earlySpawn = true;
        internal static GameObject enemyMaster;

       
        internal static GameObject displayPrefab;
        internal static List<GameObject> masterPrefabs = new List<GameObject>();

        public const string Suckplant_PREFIX = SuckplantPlugin.DEVELOPER_PREFIX + "_Suckplant_BODY_";
        //used when registering your survivor's language tokens
        public override string survivorTokenPrefix => Suckplant_PREFIX;

        public static int bodyRendererIndex;

        internal static ItemDisplayRuleSet itemDisplayRuleSet;
        internal static List<ItemDisplayRuleSet.KeyAssetRuleGroup> itemDisplayRules;


        internal static void CreateCharacter()
        {

        }
        



        public override BodyInfo bodyInfo { get; set; } = new BodyInfo
        {
            bodyName = "SuckplantBody",
            bodyNameToken = SuckplantPlugin.DEVELOPER_PREFIX + "_Suckplant_BODY_NAME",
            subtitleNameToken = SuckplantPlugin.DEVELOPER_PREFIX + "_Suckplant_BODY_SUBTITLE",

            characterPortrait = Assets.mainAssetBundle.LoadAsset<Texture>("texSuckplantIcon"),
            bodyColor = Color.cyan,

            crosshair = Modules.Assets.LoadCrosshair("Standard"),
            podPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),

            maxHealth = 330f,
            healthRegen = 1.5f,
            armor = 0f,
            moveSpeed = 3f,
            damage = 20f,
            healthGrowth = 50f,

            jumpCount = 0,


        };

        public override CustomRendererInfo[] customRendererInfos { get; set; } = new CustomRendererInfo[]
        {
                new CustomRendererInfo
                {
                    childName = "SuckplantEyes",
                    //material = Materials.CreateHopooMaterial("SuckplantMat"),
                },

                new CustomRendererInfo
                {
                    childName = "Suckplant",
                }
        };



        public override UnlockableDef characterUnlockableDef => null;

        public override Type characterMainState => typeof(EntityStates.GenericCharacterMain);

        public override ItemDisplaysBase itemDisplays => new SuckplantItemDisplays();

        //if you have more than one character, easily create a config to enable/disable them like this
        public override ConfigEntry<bool> characterEnabledConfig => null; //Modules.Config.CharacterEnableConfig(bodyName);



        public override void InitializeCharacter()
        {
            base.InitializeCharacter();
            bodyPrefab.AddComponent<Components.DamageEaten>();

            RoR2.DeathRewards d = bodyPrefab.AddComponent<DeathRewards>();
            SetStateOnHurt s = bodyPrefab.GetComponent<SetStateOnHurt>();
            s.canBeStunned = false;
            s.canBeHitStunned = false;
            s.canBeFrozen = true;
            CharacterBody c = bodyPrefab.GetComponent<CharacterBody>();
            c.sprintingSpeedMultiplier = 1.25f;
            c.bodyFlags = CharacterBody.BodyFlags.ImmuneToGoo;
            CharacterMotor m = bodyPrefab.GetComponent<CharacterMotor>();
            m.mass = 450f;
            FootstepHandler f = bodyPrefab.GetComponent<ModelLocator>().modelTransform.gameObject.GetComponent<FootstepHandler>();
            f.baseFootstepString = "Play_clayBruiser_step";
            f.footstepDustPrefab = Resources.Load<GameObject>("Prefabs/GenericLargeFootstepDust");
            SfxLocator sfx = bodyPrefab.GetComponent<SfxLocator>();
            sfx.deathSound = "Play_clayBruiser_death";
            sfx.barkSound = "Play_clayBruiser_idle_VO";
            sfx.fallDamageSound = String.Empty;
            sfx.landingSound = String.Empty;

            CharacterDirection di = bodyPrefab.GetComponent<CharacterDirection>();
            di.turnSpeed = 200f;

            EntityStateMachine b = bodyPrefab.GetComponent<EntityStateMachine>();
            b.initialStateType = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Suckplant.SpawnState));
            CharacterDeathBehavior de = bodyPrefab.GetComponent<CharacterDeathBehavior>();
            de.deathState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Suckplant.DeathState));

            enemyMaster = Suckplant.CreateMaster(bodyPrefab, "SuckplantMaster");

            CreateSpawnCard();


        }


        public static GameObject CreateMaster(GameObject bodyPrefab, string masterName)
        {
            GameObject gameObject = PrefabAPI.InstantiateClone(RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/MercMonsterMaster"), "SuckplantMaster");

            gameObject.GetComponent<RoR2.CharacterMaster>().bodyPrefab = bodyPrefab;

            foreach (AISkillDriver ai in gameObject.GetComponentsInChildren<AISkillDriver>())
            {
                UnityEngine.Object.DestroyImmediate(ai);
            }

            BaseAI baseAI = gameObject.GetComponent<BaseAI>();
            baseAI.aimVectorMaxSpeed = 40;
            baseAI.aimVectorDampTime = 0.2f;

            AISkillDriver SoakDriver = gameObject.AddComponent<AISkillDriver>();
            SoakDriver.customName = "Suck";
            SoakDriver.movementType = AISkillDriver.MovementType.Stop;
            SoakDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            SoakDriver.activationRequiresAimConfirmation = false;
            SoakDriver.activationRequiresTargetLoS = true;
            SoakDriver.selectionRequiresTargetLoS = false;
            SoakDriver.maxDistance = 20f;
            SoakDriver.minDistance = 0f;
            SoakDriver.requireSkillReady = true;
            SoakDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            SoakDriver.ignoreNodeGraph = false;
            SoakDriver.moveInputScale = 0f;
            SoakDriver.driverUpdateTimerOverride = 13f;
            SoakDriver.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;
            SoakDriver.minTargetHealthFraction = float.NegativeInfinity;
            SoakDriver.maxTargetHealthFraction = float.PositiveInfinity;
            SoakDriver.minUserHealthFraction = float.NegativeInfinity;
            SoakDriver.maxUserHealthFraction = float.PositiveInfinity;
            SoakDriver.skillSlot = SkillSlot.Secondary;

            AISkillDriver ShootDriver = gameObject.AddComponent<AISkillDriver>();
            ShootDriver.customName = "Shoot";
            ShootDriver.movementType = AISkillDriver.MovementType.Stop;
            ShootDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            ShootDriver.activationRequiresAimConfirmation = true;
            ShootDriver.activationRequiresTargetLoS = false;
            ShootDriver.selectionRequiresTargetLoS = false;
            ShootDriver.maxDistance = 30f;
            ShootDriver.minDistance = 0f;
            ShootDriver.requireSkillReady = true;
            ShootDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            ShootDriver.ignoreNodeGraph = false;
            ShootDriver.moveInputScale = 0f;
            ShootDriver.driverUpdateTimerOverride = 6f;
            ShootDriver.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;
            ShootDriver.minTargetHealthFraction = float.NegativeInfinity;
            ShootDriver.maxTargetHealthFraction = float.PositiveInfinity;
            ShootDriver.minUserHealthFraction = float.NegativeInfinity;
            ShootDriver.maxUserHealthFraction = float.PositiveInfinity;
            ShootDriver.skillSlot = SkillSlot.Primary;


            AISkillDriver TPDriver = gameObject.AddComponent<AISkillDriver>();
            TPDriver.customName = "Teleport";
            TPDriver.movementType = AISkillDriver.MovementType.Stop;
            TPDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            TPDriver.activationRequiresAimConfirmation = false;
            TPDriver.activationRequiresTargetLoS = false;
            TPDriver.selectionRequiresTargetLoS = false;
            TPDriver.maxDistance = float.PositiveInfinity;
            TPDriver.minDistance = 50f;
            TPDriver.requireSkillReady = true;
            TPDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            TPDriver.ignoreNodeGraph = true;
            TPDriver.moveInputScale = 0f;
            TPDriver.driverUpdateTimerOverride = -1f;
            TPDriver.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;
            TPDriver.minTargetHealthFraction = float.NegativeInfinity;
            TPDriver.maxTargetHealthFraction = float.PositiveInfinity;
            TPDriver.minUserHealthFraction = float.NegativeInfinity;
            TPDriver.maxUserHealthFraction = float.PositiveInfinity;
            TPDriver.skillSlot = SkillSlot.Utility;


            ContentPacks.masterPrefabs.Add(gameObject);

            return gameObject;
        }







        private static void CreateSpawnCard()
        {
            RoR2.CharacterSpawnCard characterSpawnCard = ScriptableObject.CreateInstance<RoR2.CharacterSpawnCard>();
            characterSpawnCard.name = "cscAxe";
            characterSpawnCard.prefab = Suckplant.enemyMaster;
            characterSpawnCard.sendOverNetwork = true;
            characterSpawnCard.hullSize = HullClassification.Golem;
            characterSpawnCard.nodeGraphType = MapNodeGroup.GraphType.Ground;
            characterSpawnCard.requiredFlags = NodeFlags.None;
            characterSpawnCard.forbiddenFlags = NodeFlags.TeleporterOK;
            characterSpawnCard.directorCreditCost = Suckplant.spawnCost;
            characterSpawnCard.occupyPosition = false;
            characterSpawnCard.loadout = new RoR2.SerializableLoadout();
            characterSpawnCard.noElites = false;
            characterSpawnCard.forbiddenAsBoss = false;

            DirectorCard card = new DirectorCard
            {
                spawnCard = characterSpawnCard,
                selectionWeight = 1,
                preventOverhead = false,
                minimumStageCompletions = 2,
                spawnDistance = DirectorCore.MonsterSpawnDistance.Close
            };

            DirectorAPI.DirectorCardHolder SuckplantCard  = new DirectorAPI.DirectorCardHolder
            {
                Card = card,
                MonsterCategory = DirectorAPI.MonsterCategory.Minibosses,
            };

            DirectorAPI.Helpers.AddNewMonsterToStage(SuckplantCard, false, DirectorAPI.Stage.TitanicPlains);
            DirectorAPI.Helpers.AddNewMonsterToStage(SuckplantCard, false, DirectorAPI.Stage.SunderedGrove);
            DirectorAPI.Helpers.AddNewMonsterToStage(SuckplantCard, false, DirectorAPI.Stage.SirensCall);
        }
























        public override void InitializeSkills()
        {
            Modules.Skills.CreateSkillFamilies(bodyPrefab);
            string prefix = SuckplantPlugin.DEVELOPER_PREFIX;

            #region Primary
            //Creates a skilldef for a typical primary 
            SkillDef SoakSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_Suckplant_BODY_PRIMARY_GUN_NAME",
                skillNameToken = prefix + "_Suckplant_BODY_PRIMARY_GUN_NAME",
                skillDescriptionToken = prefix + "_Suckplant_BODY_PRIMARY_GUN_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texSecondaryIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.SoakAttack)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 1,
                baseRechargeInterval = 25f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = false,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
                keywordTokens = new string[] { "KEYWORD_AGILE" }
            });

            Modules.Skills.AddSecondarySkills(bodyPrefab, SoakSkillDef);
            #endregion

            #region Secondary
            SkillDef shootSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_Suckplant_BODY_SECONDARY_GUN_NAME",
                skillNameToken = prefix + "_Suckplant_BODY_SECONDARY_GUN_NAME",
                skillDescriptionToken = prefix + "_Suckplant_BODY_SECONDARY_GUN_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texSecondaryIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Shoot)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 1,
                baseRechargeInterval = 6f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Any,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
                keywordTokens = new string[] { "KEYWORD_AGILE" }
            });

            Modules.Skills.AddPrimarySkills(bodyPrefab, shootSkillDef);
            #endregion

            #region Utility
            SkillDef rollSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "Teleport",
                skillNameToken = prefix + "_Suckplant_BODY_Utility_GUN_NAME",
                skillDescriptionToken = prefix + "Teleport",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texSecondaryIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Teleport)),
                activationStateMachineName = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = 20f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = false,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
                keywordTokens = new string[] { "KEYWORD_AGILE" }
         
        });

            Modules.Skills.AddUtilitySkills(bodyPrefab, rollSkillDef);
            #endregion

            #region Special
            SkillDef bombSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {

            });

            Modules.Skills.AddSpecialSkills(bodyPrefab, bombSkillDef);
            #endregion
        }

        public override void InitializeSkins()
        {
            GameObject model = bodyPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = model.AddComponent<ModelSkinController>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer;

            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            SkinDef defaultSkin = Modules.Skins.CreateSkinDef(SuckplantPlugin.DEVELOPER_PREFIX + "_Suckplant_BODY_DEFAULT_SKIN_NAME",
                Assets.mainAssetBundle.LoadAsset<Sprite>("texMainSkin"),
                defaultRenderers,
                mainRenderer,
                model);


            skins.Add(defaultSkin);
            #endregion

            skinController.skins = skins.ToArray();
        }

        private static CharacterModel.RendererInfo[] SkinRendererInfos(CharacterModel.RendererInfo[] defaultRenderers, Material[] materials)
        {
            CharacterModel.RendererInfo[] newRendererInfos = new CharacterModel.RendererInfo[defaultRenderers.Length];
            defaultRenderers.CopyTo(newRendererInfos, 0);

            newRendererInfos[0].defaultMaterial = materials[0];
            newRendererInfos[1].defaultMaterial = materials[1];
            newRendererInfos[2].defaultMaterial = materials[2];

            return newRendererInfos;
        }
    } 
        }