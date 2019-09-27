using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalServer.CommandProcessing
{

    #region UniversalCommand Interface

    public interface UniversalCommand
    {
        string[] CommandName { get; }
        void Invoke(string[] Args);
    }

    #endregion UniversalCommand Interface

    #region CommandLine

    public class CommandLine
    {
        #region Vars

        List<UniversalCommand> ServerCommands = new List<UniversalCommand>();

        #endregion Vars

        #region Initialization 



        #endregion Initialization

        #region AddDefaultServerCommands

        private void AddServerCommands()
        {
            
        }

        #endregion AddDefaultServerCommands

    }

    #endregion Commandline

    #region Commands

    #region CreateServer

    public class CreateServer : UniversalCommand
    {
        public string[] CommandName
        { get { return new string[] { "createserver", "create" }; } }

        public void Invoke(string[] Args)
        {
            
        }
    }

    #endregion CreateServer

    #endregion Commands
}
