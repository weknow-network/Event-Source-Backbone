using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.ConsoleTests
{
    class Subscriber : ISequenceOperations
    {
        private Subscriber()
        {

        }
        
        public static ISequenceOperations Default = new Subscriber();

        public ValueTask ActivateAsync(int id)
        {
            Console.WriteLine($"{nameof(ActivateAsync)}: {id}");
            return ValueTaskStatic.CompletedValueTask;
        }

        public ValueTask ApproveAsync(int id)
        {
            Console.WriteLine($"{nameof(ApproveAsync)}: {id}");
            return ValueTaskStatic.CompletedValueTask;
        }

        public ValueTask EarseAsync(int id)
        {
            Console.WriteLine($"{nameof(EarseAsync)}: {id}");
            return ValueTaskStatic.CompletedValueTask;
        }

        public ValueTask LoginAsync(string email, string password)
        {
            Console.WriteLine($"{nameof(LoginAsync)}: {email}");
            return ValueTaskStatic.CompletedValueTask;
        }

        public ValueTask LogoffAsync(int id)
        {
            Console.WriteLine($"{nameof(LogoffAsync)}: {id}");
            return ValueTaskStatic.CompletedValueTask;
        }

        public ValueTask RegisterAsync(User user)
        {
            Console.WriteLine($"{nameof(RegisterAsync)}: {user}");
            return ValueTaskStatic.CompletedValueTask;
        }

        public ValueTask SuspendAsync(int id)
        {
            Console.WriteLine($"{nameof(SuspendAsync)}: {id}");
            return ValueTaskStatic.CompletedValueTask;
        }

        public ValueTask UpdateAsync(User user)
        {
            Console.WriteLine($"{nameof(UpdateAsync)}: {user}");
            return ValueTaskStatic.CompletedValueTask;
        }
    }
}
