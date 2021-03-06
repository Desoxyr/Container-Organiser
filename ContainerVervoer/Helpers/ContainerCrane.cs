﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using ContainerVervoer.Exceptions;

namespace ContainerVervoer
{
    public static class ContainerCrane
    {
        private static int _maxTopWeight;
        public static void Sort(Ship ship, List<Container> containers, IConfiguration configuration)
        {
            _maxTopWeight = configuration.ContainerTopWeight;

            SortType(ship, containers, ContainerType.VaCo);
            SortType(ship, containers, ContainerType.Cooled);
            SortType(ship, containers, ContainerType.Valuable);
            SortType(ship, containers, ContainerType.Normal);
            
            PostSortingCheck(ship);
        }

        /// <summary>
        /// Sorts one ContainerType from a list of containers
        /// </summary>
        private static void SortType(Ship ship, List<Container> containers, ContainerType type)
        {
            List<Container> containersOfType = GetAllOfType(containers, type);
            SortContainers(ship, containersOfType);
        }

        /// <summary>
        /// Sorts containers from heaviest to lightest then places them on the ship
        /// </summary>
        private static void SortContainers(Ship ship, List<Container> containers)
        {
            if (!containers.Any()) return;
            
            containers = containers.OrderByDescending(c => c.Weight).ToList();
            Column[] eligiblePlaces = GetEligiblePlaces(ship, containers.First().Type);
            foreach (var container in containers)
            {
                PlaceContainer(ship, eligiblePlaces, container); 
            }
        }
        
        private static void PlaceContainer(Ship ship, Column[] eligiblePlaces, Container container)
        {
            if (ship.GetLeftSideWeight() >= ship.GetRightSideWeight())
                PlaceRightToLeft(ship, eligiblePlaces, container);
            else 
                PlaceLeftToRight(ship, eligiblePlaces, container);
        }
        
        /// <summary>
        /// Finds the best stack to put a container in starting from left to the right
        /// </summary>
        private static void PlaceLeftToRight(Ship ship, Column[] eligiblePlaces, Container container)
        {
            var bestStack = CheckLeftSide(ship, eligiblePlaces, container);

            if (ship.Width % 2 == 1 && (ship.IsBalanced() || bestStack == null))
                bestStack = CheckMiddleStack(ship, eligiblePlaces, container, bestStack);
            
            if (bestStack == null)
                bestStack = CheckRightSide(ship, eligiblePlaces, container);
            
            if (bestStack == null)
                throw new NoValidLocationException(); 

            bestStack.Add(container);
        }
        
        /// <summary>
        /// Finds the best stack to put a container in starting from right to the left
        /// </summary>
        private static void PlaceRightToLeft(Ship ship, Column[] eligiblePlaces, Container container)
        {
            Stack bestStack = CheckRightSide(ship, eligiblePlaces, container);

            if (ship.Width % 2 == 1 && (ship.IsBalanced() || bestStack == null))
                bestStack = CheckMiddleStack(ship, eligiblePlaces, container, bestStack);

            if (bestStack == null)
                bestStack = CheckLeftSide(ship, eligiblePlaces, container);
            
            if (bestStack == null)
                throw new NoValidLocationException(); 

            bestStack.Add(container);
        }
        
        private static Stack CheckMiddleStack(Ship ship, Column[] eligiblePlaces, Container container, Stack bestStack)
        {
            int index = Convert.ToInt32(Math.Floor(ship.Width / 2.0));
            bestStack = CheckForBetterStack(eligiblePlaces[index].Stacks, container, bestStack);
            return bestStack;
        }
        
        private static Stack CheckRightSide(Ship ship, Column[] eligiblePlaces, Container container)
        {
            Stack bestStack = null;
            int minimum = Convert.ToInt32(Math.Ceiling(ship.Width / 2.0));
            for (int i = minimum; i < ship.Width; i++)
            {
                bestStack = CheckForBetterStack(eligiblePlaces[i].Stacks, container, bestStack);
            }
            return bestStack;
        }

        private static Stack CheckLeftSide(Ship ship, Column[] eligiblePlaces, Container container)
        {
            Stack bestStack = null;
            for (int i = 0; i < Math.Floor(ship.Width / 2.0); i++)
            {
                bestStack = CheckForBetterStack(eligiblePlaces[i].Stacks, container, bestStack);
            }
            return bestStack;
        }


        /// <summary>
        /// Checks given stacks for a stack better than the bestStack
        /// </summary>
        /// <returns> Best stack found among the list of stacks </returns>
        private static Stack CheckForBetterStack(ReadOnlyCollection<Stack> stacks, Container container, Stack bestStack)
        {
            foreach (var stack in stacks)
            {
                if (stack.GetTopWeight() + container.Weight > _maxTopWeight)
                    continue;
                if ((container.Type == ContainerType.Valuable || container.Type == ContainerType.VaCo) &&
                    stack.ContainsValuableContainer())
                    continue;
                if (bestStack == null || bestStack.Size > stack.Size)
                    bestStack = stack;
            }
            return bestStack;
        }

        private static List<Container> GetAllOfType(List<Container> containers, ContainerType type)
        {
            return containers.Where(x => x.Type == type).ToList();
        }

        /// <summary>
        /// Checks the ship for possible stacks to place a container based on its ContainerType
        /// </summary>
        /// <returns> An array of Columns which only contain eligible stacks for the given ContainerType </returns>
        public static Column[] GetEligiblePlaces(Ship ship, ContainerType type)
        {
            if (type == ContainerType.Normal)
                return ship.Columns.ToArray();
            if (type == ContainerType.Cooled)
                return ship.Columns.Select(x => new Column(new[] {x.Stacks.First()})).ToArray();
            if (type == ContainerType.VaCo)
                return ship.Columns.Select(x => new Column(new[] {x.Stacks.First()})).ToArray();
            if (type == ContainerType.Valuable && ship.Length == 1)
                return ship.Columns.Select(x => new Column(new[] {x.Stacks.First()})).ToArray();
            if (type == ContainerType.Valuable)
                return ship.Columns.Select(x => new Column(
                    new[] {x.Stacks.First(), x.Stacks.Last()})).ToArray();

            throw new ArgumentOutOfRangeException(nameof(type), type, "Container type not configured");
        }

        /// <summary>
        /// Checks if the ship is balanced
        /// </summary>
        /// <exception cref="ImbalanceException"> Throws when the ship is imbalanced </exception>
        private static void PostSortingCheck(Ship ship)
        {
            if (!ship.IsBalanced())
            {
                throw new ImbalanceException("Not able to balance ship.");
            }
        }
    }
}