//
// Copyright (c) 2020 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using System.Collections;

namespace Windows.Devices.Spi
{
    internal sealed class SpiControllerManager
    {
        private static object s_syncLock;

        // backing field for ControllersCollection
        // to store the controllers that are open
        private static ArrayList s_controllersCollection;

        /// <summary>
        /// <see cref="SpiController"/> collection.
        /// </summary>
        /// <remarks>
        /// This collection is for internal use only.
        /// </remarks>
        internal static ArrayList ControllersCollection
        {
            get
            {
                if (s_controllersCollection == null)
                {
                    if (s_syncLock == null)
                    {
                        s_syncLock = new object();
                    }

                    lock (s_syncLock)
                    {
                        if (s_controllersCollection == null)
                        {
                            s_controllersCollection = new ArrayList();
                        }
                    }
                }

                return s_controllersCollection;
            }

            set
            {
                s_controllersCollection = value;
            }
        }
    }
}
