// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Microsoft.VisualStudio.Debugger.DebugAdapterHost.Interfaces;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Serialization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;

namespace SampleDebugAdapter.VSIX.CustomProtocolExtension
{
    /// <summary>
    /// Sample class demonstrating the implementation of a custom protocol extension.  The Debug Adapter Host will create an
    /// instance of this class while starting the Sample Debug Adapter, based on the "CustomProtocolExtension" registration
    /// in the extension's .pkgdef file.
    /// </summary>
    public class SampleExtension : ICustomProtocolExtension
    {
        private IDebugAdapterHostContext context;

        #region IDebugAdapterHostComponent Implementation

        public void Initialize(IDebugAdapterHostContext context)
        {
            // Save the context object provided by the Debug Adapter Host so we can use it to access services later
            this.context = context;
        }

        #endregion

        #region ICustomProtocolExtension Implementation

        public void RegisterCustomMessages(ICustomMessageRegistry registry, IProtocolHostOperations hostOperations)
        {
            // Register our custom request
            registry.RegisterClientRequestType<PromptRequest, PromptArgs, PromptResponse>(this.OnPromptRequest);

            // Advertise support for the custom request
            registry.SetInitializeRequestProperty("supportsPromptRequest", true);
        }

        #endregion

        #region "prompt" Request Handler

        private void OnPromptRequest(IRequestResponder<PromptArgs, PromptResponse> responder)
        {
            IVsUIShell shell = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;

            try
            {
                this.context.Logger.Log("Responding to 'prompt' request.");

                shell.EnableModeless(0);
                DialogResult result = MessageBox.Show(responder.Arguments.Message, "Prompt from Debug Adapter", MessageBoxButtons.OKCancel);

                responder.SetResponse(
                    new PromptResponse()
                    {
                        Response = result == DialogResult.OK ? PromptResponse.ResponseValue.OK : PromptResponse.ResponseValue.Cancel
                    });
            }
            finally
            {
                shell.EnableModeless(1);
            }
        }

        #endregion

        #region "prompt" Request Classes

        internal class PromptArgs
        {
            [JsonProperty("message")]
            public string Message { get; set; }
        }

        internal class PromptResponse : ResponseBody
        {
            public enum ResponseValue
            {
                [EnumMember(Value = "ok")]
                OK,
                [EnumMember(Value = "cancel")]
                Cancel,
                [DefaultEnumValue]
                Unknown = Int32.MaxValue
            }

            public ResponseValue Response { get; set; }
        }

        internal class PromptRequest : DebugClientRequestWithResponse<PromptArgs, PromptResponse>
        {
            public PromptRequest() : base("prompt")
            {
            }

            public PromptRequest(string message) : this()
            {
                this.Args.Message = message;
            }
        }

        #endregion
    }
}
