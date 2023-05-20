using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedDodoServer;

public class ControlSetup : MonoBehaviour {
    
    public static ControlSetup Instance;

    void Awake () {
		if (Instance == null)
        {
            Instance = this;
        }
	}

    public bool GetButtonDown(int player, int button)
    {
        bool result = false;

        if (GetClientsCount() > 0)
        {
            if (player < GetClientsCount())
            {
                PlayerInput playerInput = Server.instance.connectedControllers[player].playerInput;

                switch (button)
                {
                    case 0:
                        result = playerInput.aButton;
                        break;
                    case 1:
                        result = playerInput.bButton;
                        break;
                    case 2:
                        result = playerInput.xButton;
                        break;
                    case 3:
                        result = playerInput.yButton;
                        break;
                    case 7:
                        result = playerInput.start;
                        break;
                }
            }
        }

        return result;
    }

    public float GetAxis(int player, string axis)
    {
        if (GetClientsCount() > 0)
        {
            if (player < GetClientsCount())
            {
                PlayerInput playerInput = Server.instance.connectedControllers[player].playerInput;

                if (axis == "Horizontal")
                {
                    return playerInput.leftStick.Horizontal;
                } else if (axis == "Vertical")
                {
                    return playerInput.leftStick.Vertical;
                }
            }

            return 0;
        }
        else
        {
            return 0f;
        }
    }

    public int GetClientsCount()
    {
        //return TCPServer.instance.clientList.Count;
        return Server.instance.connectedControllers.Count;

    }

    private ConnectedController GetClient(int id)
    {
        if (id < 0 || id >= Server.instance.connectedControllers.Count)
        {
            return null;
        }

        return Server.instance.connectedControllers[id];
    }

    public string GetClientName(int id)
    {
        ConnectedController client = GetClient(id);

        if (client != null)
        {
            return client.playerInput.clientData.name;
        } else
        {
            return "";
        }
    }
}
