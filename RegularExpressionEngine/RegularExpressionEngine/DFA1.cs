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
        public C5.HashSet<state> to = new C5.HashSet<state>();
        public input inp;

        public DFA_info(input inp) {
            this.inp = inp;
        }

        public DFA_info(input inp, C5.HashSet<state> to)
        {
            this.inp = inp;
            this.to = to;
        }

        public DFA_info() { }
    }
    class DFA1
    {
        public List<C5.HashSet<state>> states = new List<C5.HashSet<state>>();
        public C5.HashSet<state> initial = new C5.HashSet<state>();
        public List<C5.HashSet<state>> final = new List<C5.HashSet<state>>();
        public state? halt = null;
        public SortedArray<input> inputs = new SortedArray<input>();
        public Dictionary<C5.HashSet<state>, List<DFA_info>> transTable = new Dictionary<C5.HashSet<state>, List<DFA_info>>();
        public object[] statesArray;


        public DFA1(NFA nfa) {
            inputs = nfa.inputs;
        }
    }


}
