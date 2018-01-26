//-------------------------------------------------------------------------------------------------
// <copyright file="CabinetBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Builds cabinets using multiple threads.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Diagnostics;
    using System.Collections;
    using System.Globalization;
    using Microsoft.Tools.WindowsInstallerXml.Cab;

    /// <summary>
    /// This implements a thread pool that generates cabinets with multiple threads.
    /// Unlike System.Threading.ThreadPool, it waits until all threads are finished.
    /// </summary>
    internal sealed class CabinetBuilder
    {
        private Queue cabinetWorkItems;
        private object lockObject;
        private int threadCount;

        /// <summary>
        /// Instantiate a new CabinetBuilder.
        /// </summary>
        /// <param name="threadCount">number of threads to use</param>
        public CabinetBuilder(int threadCount)
        {
            if (0 >= threadCount)
            {
                throw new ArgumentOutOfRangeException("threadCount");
            }

            this.cabinetWorkItems = new Queue();
            this.lockObject = new object();

            this.threadCount = threadCount;
        }

        /// <summary>
        /// Event for messages.
        /// </summary>
        public event MessageEventHandler Message;

        /// <summary>
        /// Enqueues a CabinetWorkItem to the queue.
        /// </summary>
        /// <param name="cabinetWorkItem">cabinet work item</param>
        public void Enqueue(CabinetWorkItem cabinetWorkItem)
        {
            this.cabinetWorkItems.Enqueue(cabinetWorkItem);
        }

        /// <summary>
        /// Create the queued cabinets.
        /// </summary>
        public void CreateQueuedCabinets()
        {
            // don't create more threads than the number of cabinets to build
            if (this.cabinetWorkItems.Count < this.threadCount)
            {
                this.threadCount = this.cabinetWorkItems.Count;
            }

            if (0 < this.threadCount)
            {
                Thread[] threads = new Thread[this.threadCount];

                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i] = new Thread(new ThreadStart(this.ProcessWorkItems));
                    threads[i].Start();
                }

                // wait for all threads to finish
                foreach (Thread thread in threads)
                {
                    thread.Join();
                }
            }
        }

        /// <summary>
        /// This function gets called by multiple threads to do actual work.
        /// It takes one work item at a time and calls this.CreateCabinet().
        /// It does not return until cabinetWorkItems queue is empty
        /// </summary>
        private void ProcessWorkItems()
        {
            try
            {
                while (true)
                {
                    CabinetWorkItem cabinetWorkItem;

                    lock (this.cabinetWorkItems)
                    {
                        // check if there are any more cabinets to create
                        if (0 == this.cabinetWorkItems.Count)
                        {
                            break;
                        }

                        cabinetWorkItem = (CabinetWorkItem)this.cabinetWorkItems.Dequeue();
                    }

                    // create a cabinet
                    this.CreateCabinet(cabinetWorkItem);
                }
            }
            catch (WixException we)
            {
                this.OnMessage(we.Error);
            }
            catch (Exception e)
            {
                this.OnMessage(WixErrors.UnexpectedException(e.Message, e.GetType().ToString(), e.StackTrace));
            }
        }

        /// <summary>
        /// Creates a cabinet using the wixcab.dll interop layer.
        /// </summary>
        /// <param name="cabinetWorkItem">CabinetWorkItem containing information about the cabinet to create.</param>
        private void CreateCabinet(CabinetWorkItem cabinetWorkItem)
        {
            this.OnMessage(WixVerboses.CreateCabinet(cabinetWorkItem.CabinetFile));

            // create the cabinet file
            string cabinetFileName = Path.GetFileName(cabinetWorkItem.CabinetFile);
            string cabinetDirectory = Path.GetDirectoryName(cabinetWorkItem.CabinetFile);

            try
            {
                using (WixCreateCab cab = new WixCreateCab(cabinetFileName, cabinetDirectory, 0, 0, cabinetWorkItem.CompressionLevel))
                {
                    foreach (FileRow fileRow in cabinetWorkItem.FileRows)
                    {
                        cab.AddFile(fileRow.Source, fileRow.File);
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                throw new WixException(WixErrors.FileNotFound(null, e.FileName));
            }
        }

        /// <summary>
        /// Sends a message to the message delegate if there is one.
        /// </summary>
        /// <param name="mea">Message event arguments.</param>
        private void OnMessage(MessageEventArgs mea)
        {
            WixErrorEventArgs errorEventArgs = mea as WixErrorEventArgs;

            if (null != this.Message)
            {
                lock (this.lockObject)
                {
                    this.Message(this, mea);
                }
            }
            else if (null != errorEventArgs)
            {
                throw new WixException(errorEventArgs);
            }
        }
    }
}

