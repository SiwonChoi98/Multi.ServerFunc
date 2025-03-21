using UnityEngine;
using System.Collections.Generic;
using Photon.Realtime;
public class Party
{
    public int PartyID { get; private set; }
    public Player Leader { get; private set; }
    public List<Player> Members { get; private set; }

    public Party(int partyID, Player leader)
    {
        PartyID = partyID;
        Leader = leader;
        Members = new List<Player>() { leader };
    }

    public bool AddMember(Player player)
    {
        if(!Members.Contains(player))
        {
            Members.Add(player);
            return true;
        }
        return false;
    }

    public bool RemoveMember(Player player)
    {
        if(Members.Contains(player))
        {
            Members.Remove(player);
            if(player == Leader && Members.Count > 0)
            {
                Leader = Members[0];
            }
            return true;
        }
        return false;
    }

    public bool IsMember(Player player)
    {
        return Members.Contains(player);
    }

    public void DisbandParty()
    {
        Members.Clear();
    }
}
