using UnityEngine;
using System.Collections.Generic;
using Photon.Realtime;
public class Party
{
    public int PartyID { get; private set; }
    
    //using Photon.Realtime 네임스페이스로 인해서 Player로 사용됨 그게 아니면 계속 붙여줘야함
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
