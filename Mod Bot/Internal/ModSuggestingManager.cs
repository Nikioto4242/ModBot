﻿using ModLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using TwitchChatter;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace InternalModBot
{
    /// <summary>
    /// Used by Mod-Bot to control the Twich mode mod suggesting
    /// </summary>
    public class ModSuggestingManager : Singleton<ModSuggestingManager>
    {
        void Start()
        {
            TwitchChatClient.singleton.AddChatListener(new ChatMessageNotificationDelegate(OnTwitchChatMessage));
            GlobalEventManager.Instance.AddEventListener(GlobalEvents.LevelSpawned, new Action(ShowNextInSuggestedModsQueue));
        }
        
        void OnDestroy()
        {
            TwitchChatClient.singleton.RemoveChatListener(new ChatMessageNotificationDelegate(OnTwitchChatMessage));
            GlobalEventManager.Instance.RemoveEventListener(GlobalEvents.LevelSpawned, new Action(ShowNextInSuggestedModsQueue));
        }
        
        /// <summary>
        /// Shows the next in the suggested mods queue
        /// </summary>
        public void ShowNextInSuggestedModsQueue()
        {
            if (_modSuggestionQueue.Count == 0)
                return;

            ModSuggestion nextSuggestedMod = _modSuggestionQueue.Dequeue();
            StartCoroutine(suggestMod(nextSuggestedMod));
        }
        
        /// <summary>
        /// Brings up the suggest window for a multiplayer suggested mod
        /// </summary>
        /// <param name="suggesterPlayfabID"></param>
        /// <param name="modName"></param>
        /// <param name="data"></param>
        public void SuggestModMultiplayer(string suggesterPlayfabID, string modName, byte[] data)
        {
            StartCoroutine(suggestModMultiplayerCoroutine(suggesterPlayfabID, modName, data));
        }

        IEnumerator suggestModMultiplayerCoroutine(string suggesterPlayfabID, string modName, byte[] data)
        {
            string displayName = null;

            bool hasDisplayName = false;
            MultiplayerPlayerInfoManager.Instance.TryGetDisplayName(suggesterPlayfabID, delegate (string receivedDisplayName)
            {
                displayName = receivedDisplayName;
                hasDisplayName = true;
            });

            yield return new WaitUntil(() => hasDisplayName);

            if (displayName == null)
                displayName = "";

            DisplayText.text = ModBotLocalizationManager.FormatLocalizedStringFromID("mod_suggested_multiplayer", displayName, modName);

            ModSuggestionAnimator.Play("suggestMod");

            KeyCode clickedKey;
            while (true)
            {
                if (Input.GetKeyDown(KeyCode.PageDown))
                {
                    clickedKey = KeyCode.PageDown;
                    break;
                }
                if (Input.GetKeyDown(KeyCode.PageUp))
                {
                    clickedKey = KeyCode.PageUp;
                    break;
                }

                yield return 0;
            }


			//TODO: Update this to work with the new mod loading system

			/*
            if (clickedKey == KeyCode.PageUp)
            {
                ModSuggestionAnimator.Play("AcceptMod");

                if (!ModsManager.Instance.LoadMod(data, modName, false, out string error))
                {
                    debug.Log(LocalizationManager.Instance.GetTranslatedString("mod_suggested_multiplayer_load_fail"), Color.red);
                }
                else
                {
                    debug.Log(LocalizationManager.Instance.GetTranslatedString("mod_suggested_multiplayer_load_success"), Color.green);
                }
            }
            else if (clickedKey == KeyCode.PageDown)
            {
                ModSuggestionAnimator.Play("DenyMod");
            }
			*/
        }

        IEnumerator suggestMod(ModSuggestion mod)
        {
            DisplayText.text = ModBotLocalizationManager.FormatLocalizedStringFromID("mod_suggested_twitch", mod.ModName, mod.SuggesterName);

            ModSuggestionAnimator.Play("suggestMod");

            KeyCode clickedKey;
            while (true)
            {
                if (Input.GetKeyDown(KeyCode.PageDown))
                {
                    clickedKey = KeyCode.PageDown;
                    break;
                }
                if (Input.GetKeyDown(KeyCode.PageUp))
                {
                    clickedKey = KeyCode.PageUp;
                    break;
                }

                yield return 0;
            }

			// TODO: Make this work with the new mod loading system
			/*
            if (clickedKey == KeyCode.PageUp)
            {
                ModSuggestionAnimator.Play("AcceptMod");
                TwitchManager.Instance.EnqueueChatMessage("Mod accepted :)");
                UnityWebRequest webRequest = UnityWebRequest.Get(mod.Url);

                yield return webRequest.SendWebRequest();

                byte[] data = webRequest.downloadHandler.data;
                if (!ModsManager.Instance.LoadMod(data, mod.ModName, false, out string error))
                {
                    debug.Log(LocalizationManager.Instance.GetTranslatedString("mod_suggested_twitch_load_fail"), Color.red);
                    TwitchManager.Instance.EnqueueChatMessage("Suggested mod \"" + mod.ModName + "\" failed to load, the link may be incorrect or the mod could be outdated.");
                }
            }
            if (clickedKey == KeyCode.PageDown)
            {
                ModSuggestionAnimator.Play("DenyMod");
                TwitchManager.Instance.EnqueueChatMessage("Mod denied :(");
            }
			*/

        }

        /// <summary>
        /// Gets called whenever anyone in twich chat sends a message
        /// </summary>
        /// <param name="msg"></param>
        public void OnTwitchChatMessage(ref TwitchChatMessage msg)
        {
            string lowerText = msg.chatMessagePlainText;
            if (!lowerText.StartsWith("!"))
                return;

            string[] subCommands = lowerText.Split(' ');

            if (subCommands[0].ToLower() == "!modsuggest")
            {
                if (subCommands.Length < 3)
                {
                    TwitchManager.Instance.EnqueueChatMessage("Usage: !modsuggest <mod_name> <mod_link>");
                    return;
                }

                string url = subCommands[2];
                string suggester = "<color=" + msg.userNameColor + ">" + msg.userName + "</color>";
                string modName = subCommands[1];
                ModSuggestion suggestedMod = new ModSuggestion(modName, suggester, url);
                _modSuggestionQueue.Enqueue(suggestedMod);

                if (!GameFlowManager.Instance.IsInCombat())
                {
                    ModSuggestion modSuggestion = _modSuggestionQueue.Dequeue();
                    StartCoroutine(suggestMod(modSuggestion));
                }

                TwitchManager.Instance.EnqueueChatMessage("Mod suggested!");
                return;
            }
            if (subCommands[0].ToLower() == "!mods")
            {
                List<Mod> mods = ModsManager.Instance.GetAllLoadedActiveMods();

                string allMods = "";
                for(int i = 0; i < mods.Count; i++)
                {
                    Mod mod = mods[i];

                    allMods += mod.ModInfo.DisplayName;

                    if (i != mods.Count-1)
                    {
                        allMods += ", ";
                    }
                    
                }

                TwitchManager.Instance.EnqueueChatMessage("MrDestructoid " + msg.userName + " Loaded mods: " + allMods + " MrDestructoid");
                return;
            }

        }

        /// <summary>
        /// The animator that plays the slide in and out animation
        /// </summary>
        public Animator ModSuggestionAnimator;

        /// <summary>
        /// Text text display where all info will be displayed
        /// </summary>
        public Text DisplayText;

        Queue<ModSuggestion> _modSuggestionQueue = new Queue<ModSuggestion>();

        struct ModSuggestion
        {
            public ModSuggestion(string modName, string suggesterName, string url)
            {
                ModName = modName;
                SuggesterName = suggesterName;
                Url = url;
            }

            public string ModName;
            public string SuggesterName;
            public string Url;

        }
    }

}
