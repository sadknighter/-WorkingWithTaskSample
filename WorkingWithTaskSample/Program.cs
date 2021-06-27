using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkingWithTaskSample
{
    class Program
    {
        private static readonly CancellationTokenSource FirstTokenSource = new CancellationTokenSource();
        private static readonly CancellationTokenSource SecondTokenSource = new CancellationTokenSource();
        private static readonly CancellationTokenSource ThirdTokenSource = new CancellationTokenSource();

        static void Main()
        {
            WorkingWithTasks();
            Console.ReadKey();
        }

        static void WorkingWithTasks()
        {
            var customTaskScheduler = new CustomTaskScheduler();

            Task.Run(() => FirstTaskBody())

                .ContinueWith(
                    value => { RegardlessTaskBody(); },
                    TaskContinuationOptions.None)

                .ContinueWith(
                    value => { OnFinishedWithoutSuccessTaskBody(); },
                    FirstTokenSource.Token,
                    TaskContinuationOptions.NotOnRanToCompletion,
                    TaskScheduler.Default)

                .ContinueWith(
                    value => { OnFaultedSecondTaskBody(); },
                    SecondTokenSource.Token,
                    TaskContinuationOptions.OnlyOnFaulted,
                    TaskScheduler.Current)

                .ContinueWith(
                    antecedent => { OnCanceledTaskBody(); },
                    ThirdTokenSource.Token,
                    TaskContinuationOptions.OnlyOnCanceled,
                    customTaskScheduler);
        }

        static void FirstTaskBody()
        {
            Console.WriteLine("Task 1 was started");

            if (!FirstTokenSource.Token.IsCancellationRequested)
            {
                throw new Exception("Generated error of executed task");
            }

            Thread.Sleep(3000);
            Console.WriteLine("Task 1 was successfully finished");
        }

        static void RegardlessTaskBody()
        {
            Thread.Sleep(1000);
            Console.WriteLine("Task 2: I don't care about parent task");
            var token = FirstTokenSource.Token;

            if (!token.IsCancellationRequested) // Отмена не запрошена
            {
                throw new OperationCanceledException(token);
            }
        }

        static void OnFinishedWithoutSuccessTaskBody()
        {
            Thread.Sleep(1000);
            Console.WriteLine("Task 3: I started because my parent task is finished without success");

            var token = SecondTokenSource.Token;

            if (!token.IsCancellationRequested) // Отмена не запрошена
            {
                throw new OperationCanceledException(token);
            }
        }

        static void OnFaultedSecondTaskBody()
        {
            Thread.Sleep(1000);
            Console.WriteLine("Task 4: I started because my parent task is faulted");
            SecondTokenSource.Cancel();
            var token = SecondTokenSource.Token;

            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }
        }

        static void OnCanceledTaskBody()
        {
            Thread.Sleep(1000);
            Console.WriteLine("Parent task was canceled.");
        }
    }
}
