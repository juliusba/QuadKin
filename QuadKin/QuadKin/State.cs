using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuadKin
{
    public enum State
    {
        NoConnection,
        Initializing,
        Ready
    }

    public delegate void StateChangeHandler(State state);

    public abstract class StateClass
    {
        private State _state = State.Initializing;
        public State State
        {
            get { return _state; }
            protected set
            {
                _state = value;
                if(stateChanged != null) stateChanged(_state);
            }
        }

        public event StateChangeHandler stateChanged;
    }
}
