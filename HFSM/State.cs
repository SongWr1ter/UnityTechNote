
using System.Collections.Generic;

namespace HSM
{
    public abstract class State
    {
        public readonly StateMachine Machine;
        public readonly State Parent;
        public State ActiveChild;

        public State(StateMachine machine, State parent)
        {
            Machine = machine;
            Parent = parent;
        }
        
        protected virtual State GetInitialState() => null; // 获取初始子状态，如果是null就说明自己是叶子节点
        
        protected virtual State GetTransition() => null; // 是否需要切换状态,null代表不切换
        
        //lifecycle hooks
        protected virtual void OnEnter() { }
        protected virtual void OnExit() { }
        protected virtual void OnUpdate(float dt) { }

        internal void Enter()
        {
            if (Parent != null) Parent.ActiveChild = this;
            OnEnter();
            
            State init = GetInitialState();
            if(init != null) init.Enter();
        }

        internal void Exit()
        {
            if(ActiveChild != null) ActiveChild.Exit();
            ActiveChild = null;
            OnExit();
        }

        internal void Update(float dt)
        {
            State t = GetTransition();
            if (t != null)
            {
                Machine.Sequencer.RequestTranstation(this,t);
                return;
            }
            
            if(ActiveChild != null) ActiveChild.Update(dt);
            OnUpdate(dt);
        }

        public State GetLeafNode()
        {
            State s = this;
            while(s.ActiveChild != null) s = s.ActiveChild;
            return s;
        }

        public IEnumerable<State> PathToRoot()
        {
            for(State s = this;s != null; s = s.Parent) yield return s;
        }
    }

    public class StateMachine
    {
        public readonly State Root;
        public readonly TransitionSequencer Sequencer;
        bool started = false;

        public StateMachine(State root)
        {
            Root = root;
            Sequencer = new TransitionSequencer(this);
            
        }

        public void Start()
        {
            if (started) return;
            
            started = true;
            Root.Enter();
        }

        public void Tick(float dt)
        {
            if(!started) Start();
            InternalTick(dt);
        }

        internal void InternalTick(float dt)
        {
            Root.Update(dt);
        }

        public void ChangeState(State from, State to)
        {
            if (from == to || from == null || to == null) return;
            
            State lca = TransitionSequencer.LCA(from, to);
            
            //Exit
            for (State s = from; s != lca; s = s.Parent)
            {
                s.Exit();
            }
            
            //Enter
            var stack = new Stack<State>();
            for (State s = to; s != lca; s = s.Parent)
            {
                stack.Push(s);
            }

            while (stack.Count > 0)
            {
                stack.Pop().Enter();
            }
        }
    }

    public class TransitionSequencer
    {
        public readonly StateMachine Machine;

        public TransitionSequencer(StateMachine machine)
        {
            Machine = machine;
        }

        public void RequestTranstation(State from, State to)
        {
            Machine.ChangeState(from, to);
        }
        // lowest common ancestor of a and b
        public static State LCA(State a, State b)
        {
            var hash = new HashSet<State>();
            for (var s = a; s != null; s = s.Parent) hash.Add(s);

            for (var s = b; s != null; s = s.Parent)
            {
                if (hash.Contains(s))
                {
                    return s;
                }
            }
            return null;
        }
}
}