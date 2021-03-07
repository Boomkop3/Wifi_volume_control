using System;

namespace not_a_keylogger
{
    class ChangeResponder<T>
    {
        private T obj;
        private bool state;
        public ChangeResponder(T obj, bool state = false)
        {
            this.obj = obj;
            this.state = state;
        }
        public (bool pressed, bool released) check(bool _state)
        {
            // Console.WriteLine($"internal: {this.state}, new: {_state}");
            if (this.state != _state)
            {
                this.state = _state;
                return (_state, !_state);
            }
            return (false, false);
        }
        public T getObject()
        {
            return obj;
        }
    }
}
