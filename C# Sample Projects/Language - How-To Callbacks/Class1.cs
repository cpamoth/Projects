//Copyright (C) 2002 Microsoft Corporation
//All rights reserved.
//THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
//EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
//MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//Requires the Trial or Release version of Visual Studio .NET Professional (or greater).

delegate void Delegate1();

internal class Class1
{
    //This field holds an interface reference for calling back to the client
    private ICallback icb;

    //This field holds a delegate reference for calling back into the client

    private Delegate1 del1;

    public void RegisterInterFaceForCallback(ICallback cb)
	{
		//this method receives a reference to the client.
        //Register the client for the callback
        icb = cb;
    }

    public void UnRegisterInterfaceForCallback()
	{
        //Unregister the client 
        icb = null;
    }

    public void UseInterfaceCallback()
	{
        //This method uses the registered client ref to call back 
        //into the client

		//Make sure something is registered first
        if (icb != null)
		{
            //Call back into the client code
            icb.CallbackMethod();
        }
    }

    public void RegisterDelegateForCallback(Delegate1 d)
	{
        //this method receives a reference to the client.
        //Register the client for the callback
		del1 = d;
    }

    public void UnRegisterDelegateForCallback()
	{
        //Unregister the client 
        del1 = null;
    }

    public void UseDelegateCallback()
	{
        //This method uses the registered client ref to call back 
        //into the client
        //Make sure something is registered first
        if (del1 != null)
		{
            //Call back into the client code
            del1.BeginInvoke(null, null);
        }
    }

    public void UseAsyncDelegateCallback()
	{
        //This method uses the registered client ref to call back 
        //into the client asynchronously.
        //Make sure something is registered first
        if (del1 != null)
		{
			//Call back into the client code asynchronously
            //The arguments can be ignored in this usage.
            del1.BeginInvoke(null, null);
        }
    }

    public void UseBuiltInDelegateCallback()
	{
        //This method doesn't need a registered client. The callback is part
        //of the delegate infrastructure, and happens independently of this 
        //implementation. Note that this method isn't even aware that there
        //is a callback occurring with the delegate that makes the call.
        //With the AsyncCallback delegate, registration/unregistration is needed.
        //The callback just happens based on the client's including it in the
        //parameter list.
    }
}