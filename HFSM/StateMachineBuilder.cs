using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace HSM
{
    public class StateMachineBuilder
    {
        private readonly State Root;

        public StateMachineBuilder(State root,StateMachine machine)
        {
            Root = root;
        }

        public StateMachine Build()
        {
            var m = new StateMachine(Root);
            Wire(Root,m,new HashSet<State>());
            return m;
        }
        // 设置State中Machine
        void Wire(State s, StateMachine m, HashSet<State> visited)
        {
            if(s == null) return;
            if(!visited.Add(s)) return; // 已经有s
            
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var machineField = s.GetType().GetField("Machine",flags);
            if (machineField != null)
            {
                machineField.SetValue(s,m); // 设置s中Machine字段的值为m
            }
            //下面这一串操作等于child = s.ActiveChild;递归(child,m,visted)
            foreach (var fld in s.GetType().GetFields(flags))
            {
                if (!s.GetType().IsAssignableFrom(fld.FieldType)) continue; //能不能把s赋给fld
                if (fld.Name == "Parent") continue;
                
                var child = (State) fld.GetValue(s);
                if (child == null) continue;
                if (!ReferenceEquals(child.Parent,s)) continue; // child的parent不是s,跳过
                
                Wire(child, m, visited);
            }
        }
    }

}
