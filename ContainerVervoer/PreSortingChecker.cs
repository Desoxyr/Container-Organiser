﻿using System.Collections.Generic;
using System.Linq;
using ContainerVervoer.Exceptions;

namespace ContainerVervoer
{
    public static class PreSortingChecker
    {
        public static void ExecuteChecks(Ship ship, List<Container> containers)
        {
            int weight = containers.Sum(c => c.Weight);
            CheckMaxWeight(ship,weight);
            CheckMinWeight(ship,weight);
            CheckSpace(ship,containers);
            CheckVaCoSpaces(ship,containers);
        }

        private static void CheckMaxWeight(Ship ship, int weight)
        {
            if (weight > ship.MaxWeight)
            {
                throw new InvalidWeightException("Containers weigh too much for the ship. " +
                                                 $"Container weight: {weight}. Maximum ship weight: {ship.MaxWeight}.");
            }
        }

        private static void CheckMinWeight(Ship ship, int weight)
        {
            if (weight < ship.MinWeight)
            {
                throw new InvalidWeightException("Containers weigh too little. " +
                                                 $"Container weight: {weight}. Minimum ship weight: {ship.MinWeight}.");
            }
        }

        private static void CheckSpace(Ship ship, List<Container> containers)
        {
            int valuableSpaces = ContainerCrane.GetEligiblePlaces(ship, ContainerType.Valuable).Sum(c => c.Stacks.Count);
            int vaCoCount = containers.Count(c => c.Type == ContainerType.VaCo);
            int valuableCount = containers.Count(c => c.Type == ContainerType.Valuable) + vaCoCount;
            
            if (valuableCount > valuableSpaces)
            {
                throw new NotEnoughSpaceException("Not all valuable containers can be placed on the ship. " +
                                                  $"Amount of containers: {valuableCount}. Spaces: {valuableSpaces}");
            }
        }

        private static void CheckVaCoSpaces(Ship ship, List<Container> containers)
        {
            int vaCoSpaces = ContainerCrane.GetEligiblePlaces(ship, ContainerType.VaCo).Sum(c => c.Stacks.Count); 
            int vaCoContainersCount = containers.Count(c => c.Type == ContainerType.VaCo);
            if (vaCoContainersCount > vaCoSpaces)
            {
                throw new NotEnoughSpaceException("Not all valuable+cooled containers can be placed on the ship. " +
                                                  $"Amount of containers: {vaCoContainersCount}. Spaces: {vaCoSpaces}");
            }
        }
    }
}