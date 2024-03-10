using System;
using System.Text;
using UnityEngine;

namespace Utils
{
    public static class NameGenerator
    {
        private static string[] playerNames = { "Ant", "Bear", "Crow", "Dog", "Eel", "Frog", "Gopher", "Heron", "Ibex", "Jerboa", "Koala", "Llama", "Moth", "Newt", "Owl", "Puffin", "Rabbit", "Snake", "Trout", "Vulture", "Wolf", "Zebra" };

        public static string GetName(string userId)
        {
            int seed = userId.GetHashCode();
            seed *= Math.Sign(seed);

            StringBuilder nameOutput = new StringBuilder();

            int word = seed % playerNames.Length;
            nameOutput.Append(playerNames[word]);

            int number = seed % 1000;
            nameOutput.Append(number.ToString("0000"));

            return nameOutput.ToString();
        }
    }

}
