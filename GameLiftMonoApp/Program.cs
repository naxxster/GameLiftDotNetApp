using System;
using System.Threading;
using System.Collections.Generic;
using Aws.GameLift.Server;

namespace GameLiftMonoApp
{
    class MainClass
    {
        enum ExitCode : int
        {
            Success = 0,
            InvalidLogin = 1,
            InvalidFilename = 2,
            UnknownError = 10
        }

        public static int Main()
        {
            var listeningPort = 7777;
            var waitHandle = new AutoResetEvent(false);

            var initSDKOutcome = GameLiftServerAPI.InitSDK();
            if (initSDKOutcome.Success)
            {
                ProcessParameters processParameters = new ProcessParameters(
                    // OnGameSession callback
                    (gameSession) =>
                    {
                        Console.WriteLine("OnGameSession received.");
                        var activateGameSessionOutcome = GameLiftServerAPI.ActivateGameSession();
                        if (activateGameSessionOutcome.Success)
                        {
                            Console.WriteLine("ActivateGameSession success.");
                        }
                        else
                        {
                            Console.WriteLine("ActivateGameSession failure : " + activateGameSessionOutcome.Error.ToString());
                        }
                    },

                    // OnProcessTerminate callback
                    () =>
                    {
                        Console.WriteLine("OnProcessTerminate received.");
                        waitHandle.Set();
                    },

                    // OnHealthCheck
                    () =>
                    {
                        Console.WriteLine("OnHealthCheck received.");
                        return true;
                    },

                    listeningPort,

                    new LogParameters(new List<string>()
                    {
                        "/local/game/logs/myserver.log"
                    })
                );

                var processReadyOutcome = GameLiftServerAPI.ProcessReady(processParameters);
                if (processReadyOutcome.Success)
                {
                    Console.WriteLine("ProcessReady success.");
                }
                else
                {
                    Console.WriteLine("ProcessReady failure : " + processReadyOutcome.Error.ToString());
                }
            }
            else
            {
                Console.WriteLine("InitSDK failure : " + initSDKOutcome.Error.ToString());
            }

            Console.CancelKeyPress += new ConsoleCancelEventHandler(
                (object sender, ConsoleCancelEventArgs args) =>
                {
                    Console.WriteLine("The read operation has been interrupted.");
                    Console.WriteLine($"  Key pressed: {args.SpecialKey}");
                    Console.WriteLine($"  Cancel property: {args.Cancel}");

                    Console.WriteLine("Setting the Cancel property to true...");
                    args.Cancel = true;

                    Console.WriteLine($"  Cancel property: {args.Cancel}");
                    Console.WriteLine("The read operation will resume...");
                    waitHandle.Set();
                }
            );

            waitHandle.WaitOne();

            var processEndingOutcome = GameLiftServerAPI.ProcessEnding();
            if (processEndingOutcome.Success)
            {
                Console.WriteLine("ProcessEnding success.");
            }
            else
            {
                Console.WriteLine("ProcessEnding failure : " + processEndingOutcome.Error.ToString());
            }

            GameLiftServerAPI.Destroy();

            return (int)ExitCode.Success;
        }
    }
}
