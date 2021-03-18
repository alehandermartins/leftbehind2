using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Ship : MonoBehaviour
{
    public Game game;
    public MainCharacter character;
    public Location[] Locations;
    public Door door;
    public Slider shields;
    public List<string> inventory = new List<string>();

    public void Load()
    {
        game.timeKeeper.AddMainTimer("shields", 0f);
        inventory.AddRange(Enumerable.Repeat("energy", 12));
        inventory.AddRange(Enumerable.Repeat("parts", 12));

        foreach (Location location in Locations)
        {
            location.Initialize();
            string item = inventory[game.rnd.Next(0, inventory.Count)];
            location.inventory.Add(item);
            inventory.Remove(item);

            location.game = game;
            location.character = character;
            character.knowledge.Add(location.LocationID, false);
        }

        for(int i = 0; i < inventory.Count; i++)
        {
            string item = inventory[game.rnd.Next(0, inventory.Count)];

            List<Location> availableLocations = new List<Location>();
            foreach(Location location in Locations)
            {
                if (location.inventory.Amount() < 6)
                    availableLocations.Add(location);
            }

            Location selectedLocation = Locations[game.rnd.Next(0, availableLocations.Count)];
            selectedLocation.inventory.Add(item);
            inventory.Remove(item);
        }
    }

    public void OpenDoors(int locationID, bool moving = true)
    {
        door.Open(Locations[locationID], moving);
    }

    public void CloseDoors()
    {
        door.Close();
    }

    public bool DoorsEnabled()
    {
        return door.openTimer > 0f || door.closeTimer > 0f;
    }

    public void UpdateShields()
    {
        shields.value = game.timeKeeper.Get("shields");
    }

    public void Damage(int LocationID)
    {
        Locations[LocationID].Damage();
    }

    public List<int> DamageRooms(int hits)
    {
        if (game.timeKeeper.Get("shields") > 0f)
            hits -= 1;

        int hitsPerRoom = 1;
        List<int> damageableLocations = DamageableLocations(1);
        List<int> damagedLocations = new List<int>();

        while(damageableLocations.Count > 0 && damagedLocations.Count < hits)
        {
            int damagedLocation = damageableLocations[Random.Range(0, damageableLocations.Count)];
            damagedLocations.Add(damagedLocation);
            damageableLocations.Remove(damagedLocation);

            if (damageableLocations.Count == 0 && hits > damagedLocations.Count)
            {
                hitsPerRoom++;
                damageableLocations = DamageableLocations(hitsPerRoom);
            }
        }

        return damagedLocations;
    }

    private List<int> DamageableLocations(int hits)
    {
        List<int> damageableLocations = new List<int>();
        foreach (Location location in Locations)
        {
            if (location.Health() >= hits)
                damageableLocations.Add(location.LocationID);
        }

        return damageableLocations;
    }

    public void BoostShields()
    {
        game.timeKeeper.Reset("shields", 30);
    }
}
