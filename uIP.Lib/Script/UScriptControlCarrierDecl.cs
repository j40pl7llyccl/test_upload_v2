using System;

namespace uIP.Lib.Script
{
    public enum eUScriptControlCarrierSpreading
    {
        Undefine,
        Broadcast,
        Unicast,
    }

    /// <summary>
    /// MacroMethodProviderPlugin class "GET" funtion declare, like ioctl output/ read
    /// </summary>
    /// <param name="carrier">name of the get</param>
    /// <param name="bRetStatus">return the get status</param>
    /// <returns>return the data</returns>
    public delegate UDataCarrier[] fpGetPluginClassScriptControlCarrier( UScriptControlCarrier carrier, ref bool bRetStatus );
    /// <summary>
    /// MacroMethodProviderPlugin class "SET" function declare, like ioctl input/ write
    /// </summary>
    /// <param name="carrier">name of the set</param>
    /// <param name="data">parameter(s) for the set</param>
    /// <returns>status: ture as OK/ false as NG</returns>
    public delegate bool fpSetPluginClassScriptControlCarrier( UScriptControlCarrier carrier, UDataCarrier[] data );

    /// <summary>
    /// macro method parameter from a name in list "m_MacroConfigurations"
    /// </summary>
    /// <param name="carrier">The predefined configuration item in list</param>
    /// <param name="whichMacro">A macro information to be get</param>
    /// <param name="bRetStatus">Return the operation status</param>
    /// <returns>Parameters of the configurable item</returns>
    public delegate UDataCarrier[] fpGetMacroScriptControlCarrier( UScriptControlCarrier carrier, UMacro whichMacro, ref bool bRetStatus );
    /// <summary>
    ///  macro method parameter from a name in list "m_MacroConfigurations"
    /// </summary>
    /// <param name="carrier">The predefined configuration item in list</param>
    /// <param name="whichMacro">A macro information to be set</param>
    /// <param name="data">Parameter data to set</param>
    /// <returns>SetClassConfiguration status</returns>
    public delegate bool fpSetMacroScriptControlCarrier( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data );
}