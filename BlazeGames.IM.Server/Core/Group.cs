using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlazeGames.IM.Server.Core
{
    class Group
    {
        private static int GroupCounter = 0;
        private static Dictionary<int, List<int>> Groups = new Dictionary<int,List<int>>();

        public static void CreateGroup(int[] memIDs)
        {
            Groups.Add(GroupCounter, memIDs.ToList<int>());
            GroupCounter++;
        }

        public static void RemoveGroup(int grpID)
        {
            if (Groups.ContainsKey(grpID))
            {
                Groups[grpID].Clear();
                Groups.Remove(grpID);
            }
        }

        public static void AddMember(int grpID, int memID)
        {
            if (Groups.ContainsKey(grpID))
                if(!Groups[grpID].Contains(memID))
                    Groups[grpID].Add(memID);
        }

        public static void RemoveMember(int grpID, int memID)
        {
            if (Groups.ContainsKey(grpID))
                if (Groups[grpID].Contains(grpID))
                    Groups[grpID].Remove(memID);
        }

        public static void RemoveMemberAll(int memID)
        {
            foreach (List<int> group in Groups.Values)
                if (group.Contains(memID))
                    group.Remove(memID);

        }

        public static int[] GetMembers(int grpID)
        {
            if (Groups.ContainsKey(grpID))
                return Groups[grpID].ToArray();

            return null;
        }
    }
}
