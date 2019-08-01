﻿/*****************************************************************************
   Copyright 2018 The TensorFlow.NET Authors. All Rights Reserved.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Tensorflow
{
    public class Session : BaseSession, IPython
    {
        public Session(string target = "", Graph g = null)
            : base(target, g, null)
        {

        }

        public Session(IntPtr handle, Graph g = null)
            : base("", g, null)
        {
            _session = handle;
        }

        public Session(Graph g, SessionOptions opts = null, Status s = null)
            : base("", g, opts)
        {
            if (s == null)
                s = new Status();
        }

        public Session as_default()
        {
            tf.defaultSession = this;
            return this;
        }

        public static Session LoadFromSavedModel(string path)
        {
            var graph = c_api.TF_NewGraph();
            var status = new Status();
            var opt = c_api.TF_NewSessionOptions();

            var tags = new string[] { "serve" };
            var buffer = new TF_Buffer();

            var sess = c_api.TF_LoadSessionFromSavedModel(opt,
                IntPtr.Zero,
                path,
                tags,
                tags.Length,
                graph,
                ref buffer,
                status);

            // load graph bytes
            // var data = new byte[buffer.length];
            // Marshal.Copy(buffer.data, data, 0, (int)buffer.length);
            // var meta_graph = MetaGraphDef.Parser.ParseFrom(data);*/
            status.Check();

            return new Session(sess, g: new Graph(graph).as_default());
        }

        public static implicit operator IntPtr(Session session) => session._session;
        public static implicit operator Session(IntPtr handle) => new Session(handle);

        public void close()
        {
            Dispose();
        }

        public void Dispose()
        {
            IntPtr h = IntPtr.Zero;
            lock (this)
            {
                h = _session;
                _session = IntPtr.Zero;
            }
            if (h != IntPtr.Zero)
            {
                var status = new Status();
                c_api.TF_DeleteSession(h, status);
                status.Check(true);
            }
            
            GC.SuppressFinalize(this);
        }

        ~Session()
        {
            Dispose();
        }

        public void __enter__()
        {

        }

        public void __exit__()
        {

        }
    }
}
