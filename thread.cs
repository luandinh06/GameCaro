using System;

namespace GameCaro
{
    internal class thread
    {
        private Func<object> p;

        public thread(Func<object> p)
        {
            this.p = p;
        }
    }
}