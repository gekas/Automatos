using System;
using System.Collections;
using C5;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using state = System.Int32;
using input = System.Char;

namespace RegularExpressionEngine
{
    class DFA_info
    {
        public C5.HashSet<state> from = new C5.HashSet<state>();
        public C5.HashSet<state> to = new C5.HashSet<state>();
        public input inp;

        public DFA_info(C5.HashSet<state> from, input inp) {
            this.inp = inp;
            this.from = from;
        }

        public DFA_info() { }
    }
    class DFA1
    {
        public List<C5.HashSet<state>> states = new List<C5.HashSet<state>>();
        public C5.HashSet<state> initial = new C5.HashSet<state>();
        public C5.HashSet<state> final = new C5.HashSet<state>();
        public SortedArray<input> inputs = new SortedArray<input>();
        public ArrayList transTable = new ArrayList();

        public DFA1(NFA nfa) {
            inputs = nfa.inputs;
        }
    }


}
