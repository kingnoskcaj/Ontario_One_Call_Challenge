using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mj_ooc_challenge.Classes {
    public class Tree {
        public string master_code { get; }
        public List<string> member_code { get; }

        public Tree(string masterCode) {
            master_code = masterCode;
            member_code = new List<string>();
        }

        public bool AddMember(string memberCode) {
            try {
                member_code.Add(memberCode);
                return (true);
            } catch {
                return (false);
            }
        }

        public bool Remove(string memberCode) {
            try {
                member_code.Remove(memberCode);
                return (true);
            } catch {
                return (false);
            }
        }
    }
}
