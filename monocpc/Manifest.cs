using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace monocpc {

    public class SnaManifest {
        public int Size;
        public string File => $"sna\\{Title}.sna";
        public CheatCollection Cheats;

        public string Title;

        public override string ToString() {
            return $"{File} [{Size}]";
        }
    }

    public class Cheat {
        public string Title;
        public Poke[] Pokes;

        public bool Active;

        public override string ToString() {
            return $"{Title}";
        }

        internal void Reset() {
            Active = false;
        }
        internal void Set() {
            foreach (var poke in Pokes) poke.Set();
            Active = true;
        }
        internal void UnSet() {
            foreach (var poke in Pokes) poke.UnSet();
            Active = false;
        }

        internal void Toggle() {
            if (Active) UnSet(); else Set();
        }
    }

    public class Poke {
        public int Address;
        public byte Value;

        public byte Backup;

        public override string ToString() {
            return string.Format("{0:X},{1:X}", Address, Value);
        }

        public void Set() {
            Backup = CPC.Emulator.Instance.Memory.Peek(Address);
            CPC.Emulator.Instance.Memory.Poke(Address, Value);
        }

        public void UnSet() {
            CPC.Emulator.Instance.Memory.Poke(Address, Backup);
        }
    }

    public class Manifest {

        public static SnaManifest[] AllGames { get; private set; }
        public static SnaManifest[] Games { get; private set; }

        public static void Load(string xml) {
            XDocument doc = XDocument.Load(xml);
            AllGames = LoadSnaFiles(doc).OrderBy(g => g.Title).ToArray();

            Games = AllGames.Where(g => g.Size < 128 * 1024 || CPC.Memory.TOTAL_RAM_NUM_BANKS != 4).ToArray();
        }

        private static IEnumerable<SnaManifest> LoadSnaFiles(XDocument doc) {
            foreach (var node in doc.Descendants("Sna")) {
                yield return new SnaManifest() {
                    Title = node.Attribute("file").Value,
                    Size = Convert.ToInt32(node.Attribute("size").Value),
                    Cheats = new CheatCollection(LoadCheats(node))
                };
            }
        }

        private static IEnumerable<Cheat> LoadCheats(XElement snaNode) {
            foreach (var cheatNode in snaNode.Descendants("Cheat")) {
                yield return new monocpc.Cheat() {
                    Title = cheatNode.Attribute("title").Value,
                    Pokes = LoadPokes(cheatNode).ToArray()
                };
            }
        }
        private static IEnumerable<Poke> LoadPokes(XElement cheatNode) {
            foreach (var pokeNode in cheatNode.Descendants("Poke")) {
                yield return new monocpc.Poke() {
                    Address = ReadIntAtrribute(pokeNode, "address"),
                    Value = (byte)ReadIntAtrribute(pokeNode, "value")
                };
            }
        }

        static int ReadIntAtrribute(XElement node, string attr) {

            var value = node.Attribute(attr).Value;

            if (value.StartsWith("#")) {
                return Convert.ToInt32(value.Replace("#", "0x"), 16);
            }

            return Convert.ToInt32(value);
        }


    }

    public class CheatCollection : List<Cheat> {

        public CheatCollection(IEnumerable<Cheat> items) : base(items) {
        }

        public void Reset() {
            foreach (var cheat in this) {
                cheat.Reset();
            }
        }
    }
}
