using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EFT;
using BehaviourMachine;
using System.IO;
using System.Runtime.InteropServices;

namespace TarkovReshade
{
    public class Main : MonoBehaviour
    {
        public Main() { }

        private GameObject GameObjectHolder;

        public static Color color_0;

        private float _maxItemDistance = 50f;
        private float _maxPlayerDrawingDistance = 250f;
        private float _lootableContainerDistance = 50f;
        private float _doorDistance = 10f;

        private IEnumerable<Player> _players;
        private IEnumerable<Door> _doors;
        private IEnumerable<LootableContainer> _weaponBoxes;
        private IEnumerable<LootItem> _items;
        private IEnumerable<ExfiltrationPoint> _extractPoints;

        private float _playersNextUpdateTime;
        private float _exfilNextUpdateTime;
        private float _weaponBoxesNextUpdateTime;
        private float _itemsNextUpdateTime;
        private float _espUpdateInterval = 100f;
        private float _playerEspUpdateInterval = 1f;


        private bool _isESPMenuActive;
        private bool _showPlayersESP;
        private bool _showWeaponBoxesESP;
        private bool _showExtractESP;
        private bool _showDeadBodies;
        private bool _showAi;
        private bool _showItems;
        private bool _ScavGod;
        private bool _noRecoil;


        public void Load()
        {
            GameObjectHolder = new GameObject();
            GameObjectHolder.AddComponent<Main>();
            DontDestroyOnLoad(GameObjectHolder);
            // In GamePlayerOwner
            // if (!string.IsNullOrEmpty(class5.worldInteractiveObject_0.KeyId) && base.method_5(class5.worldInteractiveObject_0))
        }

        public void Unload()
        {
            Destroy(GameObjectHolder);
            Destroy(this);
        }

        public void UnlockDoors()
        {
            this._doors = UnityEngine.Object.FindObjectsOfType<Door>();
            foreach (Door door in _doors)
            {
                float num = Vector3.Distance(Camera.main.transform.position, door.transform.position);
                Vector3 vector = new Vector3(Camera.main.WorldToScreenPoint(door.transform.position).x, Camera.main.WorldToScreenPoint(door.transform.position).y, Camera.main.WorldToScreenPoint(door.transform.position).z);
                if (num <= this._doorDistance && (double)vector.z > 0.01)
                {
                    door.enabled = true;
                    door.DoorState = WorldInteractiveObject.EDoorState.Shut;
                }

            }
        }

        public void LockDoors()
        {
            this._doors = UnityEngine.Object.FindObjectsOfType<Door>();
            foreach (Door door in _doors)
            {
                float num = Vector3.Distance(Camera.main.transform.position, door.transform.position);
                Vector3 vector = new Vector3(Camera.main.WorldToScreenPoint(door.transform.position).x, Camera.main.WorldToScreenPoint(door.transform.position).y, Camera.main.WorldToScreenPoint(door.transform.position).z);
                if (num <= this._doorDistance && (double)vector.z > 0.01)
                {
                    door.DoorState = WorldInteractiveObject.EDoorState.Locked;
                    door.enabled = false;
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.End))
            {
                Unload();
            }
            //  if (Input.GetKeyDown(KeyCode.W))
            // {
            //      Camera.main.fieldOfView = 95f;
            //  }
            if (Input.GetKeyDown(KeyCode.Insert))
            {
                _isESPMenuActive = !_isESPMenuActive;
            }
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                IncreaseFov();
            }
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                DecreaseFov();
            }
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                UnlockDoors();
            }
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                LockDoors();
            }
        }

        private void NoRecoil()
        {
            try
            {
                foreach (Player ply in _players)
                {
                    if (ply.isLocalPlayer || ply.localPlayerAuthority)
                    {
                        ply.Weapon.Template.bFirerate = 999999;
                        ply.Weapon.Template.isFastReload = true;
                        ply.ProceduralWeaponAnimation.Shootingg.Intensity = 0;
                        ply.ProceduralWeaponAnimation.Shootingg.RecoilStrengthXY = new Vector2(0, 0);
                        ply.ProceduralWeaponAnimation.Shootingg.RecoilStrengthZ = new Vector2(0, 0);
                    }
                }
            }
            catch
            { }

        }

        private void ScavGod()
        {
            foreach (Player ply in _players)
            {
                if (ply != null)
                {
                    if (ply.isLocalPlayer || ply.hasAuthority)
                    {
                        ply.IsVisible = false;
                        ply.Skills.Metabolism.Buff = 10000;
                        ply.Physical.Sprinting.RestoreRate = 10000.0f;
                        ply.Physical.Sprinting.DrainRate = 0.0f;
                        ply.Skills.StrengthBuffSprintSpeedInc.Value = 10000f;
                        ply.Skills.StrengthBuffLiftWeightInc.Value = -100f;
                        ply.Skills.StrengthBuffMeleeCrits.Value = true;
                        ply.Skills.StrengthBuffMeleePowerInc.Value = 100f;
                        ply.Skills.StrengthBuffThrowDistanceInc.Value = 1f;
                        ply.Skills.AttentionLootSpeed.Value = 5000f;
                        ply.Skills.UniqueLoot.Factor = 500f;
                        ply.Skills.ExamineAction.Factor = 500f;
                        ply.Skills.MagDrillsLoadSpeed.Value = 500f;
                        ply.Skills.MagDrillsUnloadSpeed.Value = 500f;
                        ply.Skills.SearchBuffSpeed.Value = 500f;
                        ply.Skills.AttentionLootSpeed.Value = 500f;
                        ply.Skills.ExamineAction.Factor = 500f;
                        ply.Skills.AnySkillUp.Factor = 500f;
                    }
                }
            }
        }


        private void IncreaseFov()
        {
            Camera.main.fieldOfView += 1f;
        }

        private void DecreaseFov()
        {
            Camera.main.fieldOfView -= 1f;
        }


        private void DrawWeaponBoxesContainers()
        {
            try
            {
                foreach (LootableContainer lootableContainer in this._weaponBoxes)
                {
                    if (lootableContainer == null)
                    {
                        break;
                    }
                    float num = Vector3.Distance(Camera.main.transform.position, lootableContainer.transform.position);
                    Vector3 vector = new Vector3(Camera.main.WorldToScreenPoint(lootableContainer.transform.position).x, Camera.main.WorldToScreenPoint(lootableContainer.transform.position).y, Camera.main.WorldToScreenPoint(lootableContainer.transform.position).z);
                    if ((double)vector.z > 0.01 && num <= this._lootableContainerDistance && (lootableContainer.name.Contains("weapon_box_cover") || lootableContainer.name.Contains("safe") || lootableContainer.name.Contains("vault")))
                    {
                        var NameStyle = new GUIStyle
                        {
                            fontSize = 15
                        };
                        NameStyle.normal.textColor = Color.cyan;
                        int num2 = (int)num;
                        string name;
                        try
                        {
                            name = lootableContainer.name;
                        }
                        catch
                        {
                            name = "error";
                        }
                        string text = string.Format("{1} - {0}m", num2, name);
                        GUI.Label(new Rect(vector.x - 50f, (float)Screen.height - vector.y, 100f, 50f), text, NameStyle);
                    }
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText("weaponBoxLog.txt", ex.ToString());
            }
        }



        private void OnGUI()
        {
            if (_isESPMenuActive)
            {
                this.DrawESPMenu();
            }

            // extract esp
            if (_showExtractESP)
            {
                if (Time.time >= this._exfilNextUpdateTime)
                {
                    this._extractPoints = FindObjectsOfType<ExfiltrationPoint>();
                    this._exfilNextUpdateTime = Time.time + _espUpdateInterval;
                }
                this.DrawExtractESP();
            }


            // weapon boxes
            if (this._showWeaponBoxesESP)
            {
                if (Time.time >= this._weaponBoxesNextUpdateTime)
                {
                    this._weaponBoxes = UnityEngine.Object.FindObjectsOfType<LootableContainer>();
                    this._weaponBoxesNextUpdateTime = Time.time + this._espUpdateInterval;
                }
                this.DrawWeaponBoxesContainers();
            }


            // value items
            if (this._showItems)
            {
                if (Time.time >= this._itemsNextUpdateTime)
                {
                    this._items = UnityEngine.Object.FindObjectsOfType<LootItem>();
                    this._itemsNextUpdateTime = Time.time + this._espUpdateInterval;
                }
                this.ShowItemESP();
            }



            // string[] lootList = { "key", "usb", "phone", "gas", "money", "document", "quest", "spark", "grizzly", "sv-98", "sv98", "rsas", "salewa", "bitcoin", "dvl", "m4a1", "roler", "chain", "wallet", "RSASS", "glock", "SA-58" };

            // value items
            //   if (this._showItems)
            // {
            //      if (Time.time >= this._itemsNextUpdateTime)
            ////      {
            //this._items = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Name"); array.Any(s => s.Equals(myString))
            //        IEnumerable<LootItem> localItems = UnityEngine.Object.FindObjectsOfType<LootItem>();
            //        this._items = localItems.Where(obj => lootList.Contains(obj.name));
            //         this._itemsNextUpdateTime = Time.time + this._espUpdateInterval;
            //    }
            //     this.ShowItemESP();
            //  }


            // show players
            if (this._showPlayersESP)
            {
                if (Time.time >= this._playersNextUpdateTime)
                {
                    this._players = UnityEngine.Object.FindObjectsOfType<Player>();
                    this._playersNextUpdateTime = Time.time + this._playerEspUpdateInterval;
                }
                this.DrawPlayers();
            }

            // no recoil
            if (this._noRecoil)
            {
                this.NoRecoil();
            }
            // no recoil
            if (this._ScavGod)
            {
                this.ScavGod();
            }
        }

        private void ShowItemESP()
        {
            foreach (LootItem lootItem in this._items)
            {
                if (lootItem == null)
                {
                    break;
                }

                float num = Vector3.Distance(Camera.main.transform.position, lootItem.transform.position);
                Vector3 vector = new Vector3(Camera.main.WorldToScreenPoint(lootItem.transform.position).x, Camera.main.WorldToScreenPoint(lootItem.transform.position).y, Camera.main.WorldToScreenPoint(lootItem.transform.position).z);
                if ((double)vector.z > 0.01 && (lootItem.name.Contains("key") || lootItem.name.Contains("usb") || lootItem.name.Contains("phone") || lootItem.name.Contains("gas") || lootItem.name.Contains("money") || lootItem.name.Contains("document") || lootItem.name.Contains("quest") || lootItem.name.Contains("spark") || lootItem.name.Contains("grizzly") || lootItem.name.Contains("sv-98") || lootItem.name.Contains("sv98") || lootItem.name.Contains("rsas") || lootItem.name.Contains("salewa") || lootItem.name.Equals("bitcoin") || lootItem.name.Contains("dvl") || lootItem.name.Contains("m4a1") || lootItem.name.Contains("roler") || lootItem.name.Contains("chain") || lootItem.name.Contains("wallet") || lootItem.name.Contains("RSASS") || lootItem.name.Contains("glock") || lootItem.name.Contains("SA-58")) && num <= this._maxItemDistance && (double)vector.z > 0.01)
                {
                    int num2 = (int)num;
                    string name;
                    try
                    {
                        name = lootItem.name;
                    }
                    catch
                    {
                        name = "error";
                    }
                    var NameStyle = new GUIStyle
                    {
                        fontSize = 15
                    };
                    NameStyle.normal.textColor = Color.yellow;
                    string text = string.Format("{0} - {1}m", name, num2);
                    GUI.Label(new Rect(vector.x - 50f, (float)Screen.height - vector.y, 100f, 50f), text, NameStyle);
                }
            }
        }

        private void DrawExtractESP()
        {
            foreach (var point in _extractPoints)
            {
                if (point == null)
                {
                    break;
                }
                float distanceToObject = Vector3.Distance(Camera.main.transform.position, point.transform.position);
                var exfilContainerBoundingVector = new Vector3(
                    Camera.main.WorldToScreenPoint(point.transform.position).x,
                    Camera.main.WorldToScreenPoint(point.transform.position).y,
                    Camera.main.WorldToScreenPoint(point.transform.position).z);

                if (exfilContainerBoundingVector.z > 0.01)
                {
                    var NameStyle = new GUIStyle
                    {
                        fontSize = 15
                    };
                    NameStyle.normal.textColor = Color.green;
                    int distance = (int)distanceToObject;
                    String exfilName = point.name;
                    string boxText = $"{exfilName} - {distance}m";

                    GUI.Label(new Rect(exfilContainerBoundingVector.x - 50f, (float)Screen.height - exfilContainerBoundingVector.y, 100f, 50f), boxText, NameStyle);
                }
            }
        }


        private void DrawPlayers()
        {
            foreach (Player player in this._players)
            {
                // stop if none
                if (player == null)
                {
                    break;
                }
                float num = Vector3.Distance(Camera.main.transform.position, player.Transform.position);
                Vector3 vector = new Vector3(Camera.main.WorldToScreenPoint(player.Transform.position).x, Camera.main.WorldToScreenPoint(player.Transform.position).y, Camera.main.WorldToScreenPoint(player.Transform.position).z);
                if (num <= this._maxPlayerDrawingDistance && (double)vector.z > 0.01)
                {
                    var NameStyle = new GUIStyle
                    {
                        fontSize = 15
                    };
                    var WeapStyle = new GUIStyle
                    {
                        fontSize = 15
                    };
                    WeapStyle.normal.textColor = Color.green;
                    if (player.Profile.Info.RegistrationDate <= 0 && this._showAi && player.Profile.Health.IsAlive)
                    {
                        float x = Camera.main.WorldToScreenPoint(player.Transform.position).x;
                        float num2 = Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y + 10f;
                        float num3 = Math.Abs(Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y - Camera.main.WorldToScreenPoint(player.Transform.position).y) + 10f;
                        float num4 = num3 * 0.65f;
                        Vector3 vector2 = new Vector3(Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).x, Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y, Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).z);
                        Color color = player.Profile.Health.IsAlive ? this.GetPlayerColor(player.Side) : Color.gray;
                        string weapon = "unknown";
                        try
                        {
                            weapon = player.Weapon.Template.ShortName.Contains("Item") ? player.Weapon.Template.Name : player.Weapon.Template.ShortName;
                        }
                        catch
                        {
                            weapon = "unknown";
                        }
                        float health = player.HealthController.SummaryHealth.CurrentValue / 435f * 100f;
                        string text2 = string.Format("[{0}%] {1} [{2}m]", (int)health, "AI", (int)num);
                        NameStyle.normal.textColor = color;
                        GuiHelper.DrawBox(x - num4 / 2f, (float)Screen.height - num2, num4, num3, color);
                        GuiHelper.DrawLine(new Vector2(vector2.x - 2f, (float)Screen.height - vector2.y), new Vector2(vector2.x + 2f, (float)Screen.height - vector2.y), color);
                        GuiHelper.DrawLine(new Vector2(vector2.x, (float)Screen.height - vector2.y - 2f), new Vector2(vector2.x, (float)Screen.height - vector2.y + 2f), color);
                        GUI.skin.GetStyle(text2).CalcSize(new GUIContent(text2));
                        Vector2 vector3 = GUI.skin.GetStyle(text2).CalcSize(new GUIContent(text2));
                        GUI.Label(new Rect(vector.x - vector3.x / 2f, (float)Screen.height - num2 - 20f, 300f, 50f), text2, NameStyle);
                        GUI.Label(new Rect(vector.x - vector3.x / 2f, (float)Screen.height - num2, 300f, 50f), weapon, WeapStyle);
                    }
                    else if (player.Profile.Health.IsAlive && player.Profile.Info.RegistrationDate > 0)
                    {
                        float x = Camera.main.WorldToScreenPoint(player.Transform.position).x;
                        float num2 = Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y + 10f;
                        float num3 = Math.Abs(Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y - Camera.main.WorldToScreenPoint(player.Transform.position).y) + 10f;
                        float num4 = num3 * 0.65f;
                        Color playerColor = this.GetPlayerColor(player.Side);
                        Vector3 vector2 = new Vector3(Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).x, Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y, Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).z);
                        string weapon = "unknown";
                        try
                        {
                            weapon = player.Weapon.Template.ShortName.Contains("Item") ? player.Weapon.Template.Name : player.Weapon.Template.ShortName;
                        }
                        catch
                        {
                            weapon = "unknown";
                        }
                        float heath = player.HealthController.SummaryHealth.CurrentValue / 435f * 100f;
                        string text2 = string.Format("[{0}%] {1} [{2}m]", (int)heath, player.Profile.Info.Nickname, (int)num);
                        NameStyle.normal.textColor = playerColor;
                        GuiHelper.DrawBox(x - num4 / 2f, (float)Screen.height - num2, num4, num3, playerColor);
                        GuiHelper.DrawLine(new Vector2(vector2.x - 2f, (float)Screen.height - vector2.y), new Vector2(vector2.x + 2f, (float)Screen.height - vector2.y), playerColor);
                        GuiHelper.DrawLine(new Vector2(vector2.x, (float)Screen.height - vector2.y - 2f), new Vector2(vector2.x, (float)Screen.height - vector2.y + 2f), playerColor);
                        GUI.skin.GetStyle(text2).CalcSize(new GUIContent(text2));
                        Vector2 vector4 = GUI.skin.GetStyle(text2).CalcSize(new GUIContent(text2));
                        GUI.Label(new Rect(vector.x - vector4.x / 2f, (float)Screen.height - num2 - 20f, 300f, 50f), text2, NameStyle);
                        GUI.Label(new Rect(vector.x - vector4.x / 2f, (float)Screen.height - num2, 300f, 50f), weapon, WeapStyle);
                    }
                    else if (!player.Profile.Health.IsAlive && this._showDeadBodies)
                    {
                        float x = Camera.main.WorldToScreenPoint(player.Transform.position).x;
                        float num2 = Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y + 10f;
                        float num3 = Math.Abs(Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y - Camera.main.WorldToScreenPoint(player.Transform.position).y) + 10f;
                        float num4 = num3 * 0.65f;
                        Color playerColor = this.GetPlayerColor(player.Side);
                        Vector3 vector2 = new Vector3(Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).x, Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y, Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).z);
                        Color color = player.Profile.Health.IsAlive ? playerColor : Color.gray;
                        bool flag = player.Profile.Info.RegistrationDate <= 0;
                        string name = flag ? "AI" : player.Profile.Info.Nickname;
                        string weapon = "unknown";
                        try
                        {
                            weapon = player.Weapon.Template.ShortName.Contains("Item") ? player.Weapon.Template.Name : player.Weapon.Template.ShortName;
                        }
                        catch
                        {
                            weapon = "unknown";
                        }
                        float num5 = player.HealthController.SummaryHealth.CurrentValue / 435f * 100f;
                        string text2 = string.Format("[{0}%] {1} [{2}m]", (int)num5, name, (int)num);
                        NameStyle.normal.textColor = color;
                        GuiHelper.DrawBox(x - num4 / 2f, (float)Screen.height - num2, num4, num3, color);
                        GuiHelper.DrawLine(new Vector2(vector2.x - 2f, (float)Screen.height - vector2.y), new Vector2(vector2.x + 2f, (float)Screen.height - vector2.y), color);
                        GuiHelper.DrawLine(new Vector2(vector2.x, (float)Screen.height - vector2.y - 2f), new Vector2(vector2.x, (float)Screen.height - vector2.y + 2f), color);
                        GUI.skin.GetStyle(text2).CalcSize(new GUIContent(text2));
                        Vector2 vector3 = GUI.skin.GetStyle(text2).CalcSize(new GUIContent(text2));
                        GUI.Label(new Rect(vector.x - vector3.x / 2f, (float)Screen.height - num2 - 20f, 300f, 50f), text2, NameStyle);
                        GUI.Label(new Rect(vector.x - vector3.x / 2f, (float)Screen.height - num2, 300f, 50f), weapon, WeapStyle);
                    }
                }
            }
        }




        private Color GetPlayerColor(EPlayerSide side)
        {
            switch (side)
            {
                case EPlayerSide.Bear:
                    return Color.red;
                case EPlayerSide.Usec:
                    return Color.blue;
                case EPlayerSide.Savage:
                    return Color.white;
                default:
                    return Color.white;
            }
        }

        private void DrawESPMenu()
        {
            GUI.color = Color.black;
            GUI.Box(new Rect(100f, 100f, 260f, 280f), "");
            GUI.color = Color.white;
            GUI.Label(new Rect(180f, 110f, 150f, 20f), "Big City Money Boys");

            this._showPlayersESP = GUI.Toggle(new Rect(110f, 140f, 120f, 20f), this._showPlayersESP, "  Base ESP");
            this._showAi = GUI.Toggle(new Rect(110f, 160f, 120f, 20f), this._showAi, "  AI ESP");
            this._showDeadBodies = GUI.Toggle(new Rect(110f, 180f, 120f, 20f), this._showDeadBodies, "  Bodies ESP");
            this._maxPlayerDrawingDistance = float.Parse(GUI.TextField(new Rect(110f, 200f, 120f, 20f), this._maxPlayerDrawingDistance.ToString(), 10));

            this._showItems = GUI.Toggle(new Rect(110f, 220f, 120f, 20f), this._showItems, "  Loot");
            this._maxItemDistance = float.Parse(GUI.TextField(new Rect(110f, 240f, 120f, 20f), this._maxItemDistance.ToString(), 10));

            this._showWeaponBoxesESP = GUI.Toggle(new Rect(110f, 260f, 120f, 20f), this._showWeaponBoxesESP, "  Weapon Boxes");
            this._lootableContainerDistance = float.Parse(GUI.TextField(new Rect(110f, 280f, 120f, 20f), this._lootableContainerDistance.ToString(), 10));

            this._showExtractESP = GUI.Toggle(new Rect(110f, 300f, 120f, 20f), this._showExtractESP, "  Exit ESP");

            this._ScavGod = GUI.Toggle(new Rect(110f, 320f, 120f, 20f), this._ScavGod, "  ScavGod");
            this._noRecoil = GUI.Toggle(new Rect(110f, 340f, 120f, 20f), this._noRecoil, "  No-Recoil");
            //if (GUI.Button(new Rect(110f, 320f, 120f, 20f), "unlock doors"))
            //  UnlockDoors();

        }

        private double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2.0) + Math.Pow(y2 - y1, 2.0));
        }
    }
}

