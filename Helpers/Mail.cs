﻿using System;
using System.Linq;
using System.Numerics;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.CommonBot.Frames;
using Styx.CommonBot.Profiles;
using Styx.Pathing;
using Styx.WoWInternals;
using Templar.GUI.Tabs;

namespace Templar.Helpers {
    public class Mail {

        // ===========================================================
        // Constants
        // ===========================================================

        // ===========================================================
        // Fields
        // ===========================================================

        // ===========================================================
        // Constructors
        // ===========================================================

        // ===========================================================
        // Getter & Setter
        // ===========================================================

        // ===========================================================
        // Methods for/from SuperClass/Interfaces
        // ===========================================================

        // ===========================================================
        // Methods
        // ===========================================================

        public static void CheckBags() {
            Variables.MailList.Clear();

            foreach(var bagItem in StyxWoW.Me.BagItems.Where(bagItem =>
                !bagItem.IsSoulbound &&
                ProtectedItemSettings.Instance.ProtectedItems.All(pi => pi.Entry != bagItem.Entry) &&
                (!ProtectedItemsManager.GetAllItemIds().Contains(bagItem.Entry) || ForceMailManager.GetAllItemIds().Contains(bagItem.Entry)))) {

                switch(bagItem.Quality) {
                    case WoWItemQuality.Common:
                        if(MailSettings.Instance.MailWhites && !Variables.MailList.Contains(bagItem)) {
                            Variables.MailList.Add(bagItem);
                        }

                        break;

                    case WoWItemQuality.Uncommon:
                        if(MailSettings.Instance.MailGreens && !Variables.MailList.Contains(bagItem)) {
                            Variables.MailList.Add(bagItem);
                        }

                        break;

                    case WoWItemQuality.Rare:
                        if(MailSettings.Instance.MailBlues && !Variables.MailList.Contains(bagItem)) {
                            Variables.MailList.Add(bagItem);
                        }

                        break;

                    case WoWItemQuality.Epic:
                        if(MailSettings.Instance.MailPurples && !Variables.MailList.Contains(bagItem)) {
                            Variables.MailList.Add(bagItem);
                        }

                        break;
                }
            }
        }

        public static void HandleMailing() {
            if(string.IsNullOrEmpty(MailSettings.Instance.Recipient)) {
                CustomLog.Normal("You need to add a recipient name in the GUI.");
                TreeRoot.Stop("Add a recipient");
            }

            if(Variables.CloseMailbox != null) {
                HandleCloseMailbox();
                return;
            }

            if(Variables.FarMailbox != null) {
                HandleFarMailbox();
                return;
            }

            CustomLog.Normal("Could not find any mailbox in the cache.");
            CustomLog.Normal("Disabling mail usage to not get stuck.");
            CustomLog.Normal("Manually find a mailbox on this continent and start the bot there with the mail setting active in the GUI.");
            CustomLog.Normal("Then you can go back to your original spot again.");
            MailSettings.Instance.Mail = false;
            MailSettings.Save();
        }

        // ===========================================================
        // Inner and Anonymous Classes
        // ===========================================================

        private static void HandleFarMailbox() {
            if(Variables.FarMailbox == null) {
                CustomLog.Normal("Could not find far mailbox.");
            } else {
                if(Vector3.Distance(StyxWoW.Me.Location, Variables.FarMailbox.Location) > 30) {
                    CustomLog.Diagnostic("We have a mailbox! Distance: {0}", Vector3.Distance(Variables.FarMailbox.Location, StyxWoW.Me.Location));
                    Flightor.MoveTo(Variables.FarMailbox.Location, true);
                } else {
                    HandleCloseMailbox();
                }
            }
        }

        private static void HandleCloseMailbox() {
            if(Variables.CloseMailbox == null) {
                CustomLog.Normal("Could not find close mailbox.");
            } else {
                if(!Variables.CloseMailbox.WithinInteractRange) {
                    Flightor.MoveTo(Variables.CloseMailbox.Location, true);
                } else {
                    if(!MailFrame.Instance.IsVisible) {
                        Variables.CloseMailbox.Interact();
                    } else {
                        AttachAndSend();
                    }
                }
            }
        }

        private static void AttachAndSend() {
            if(!MailFrame.Instance.IsVisible) {
                return;
            }

            MailFrame.Instance.SwitchToSendMailTab();

            var mailCount = 0;

            foreach(var bagItem in StyxWoW.Me.BagItems.Where(bagItem => Variables.MailList.Any(mailItem => mailItem == bagItem))) {
                if(mailCount >= 12) {
                    continue;
                }

                bagItem.UseContainerItem();
                mailCount++;
                CustomLog.Normal("Attached item {0}, count = {1}", bagItem.Name, mailCount);
            }

            if(mailCount > 0) {
                Lua.DoString(String.Format(@"SendMailNameEditBox:SetText('{0}');", MailSettings.Instance.Recipient));
                Lua.DoString(String.Format(@"SendMailSubjectEditBox:SetText('{0}');", "Goodies"));
                Lua.DoString("SendMailMailButton:Click();");
            } 
        }
    }
}
