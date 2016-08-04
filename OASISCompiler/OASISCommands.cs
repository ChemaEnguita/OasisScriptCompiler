using System.Collections.Generic;

namespace OASISCompiler
{
    class OASISCommand
        {
            public string opCode; // string as used in #define
            public int nArguments;
            public List<Symbol.Types> paramList;
        }

        class OASISCommands
        {
            Dictionary<string, OASISCommand> commands;

            public OASISCommand resolve(string name)
            {
                OASISCommand c;
                if (commands.TryGetValue(name, out c))
                    return c;
                else
                    return null;
            }


            public OASISCommands()
            {
                commands = new Dictionary<string, OASISCommand>()
          {

              { "scStopScript", new OASISCommand{opCode="SC_STOP_SCRIPT", nArguments=0, paramList= new List<Symbol.Types> { } } },
              { "scRestartScript", new OASISCommand{opCode="SC_RESTART_SCRIPT", nArguments=0, paramList= new List<Symbol.Types> { } } },
              { "scWaitEvent", new OASISCommand {opCode="SC_WAIT_EVENT",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },
              { "scActorWalkTo", new OASISCommand {opCode="SC_ACTOR_WALKTO",nArguments=3, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "scActorTalk", new OASISCommand {opCode="SC_ACTOR_TALK",nArguments=3, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "scWaitForActor", new OASISCommand {opCode="SC_WAIT_FOR_ACTOR",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },
              { "scDelay", new OASISCommand {opCode="SC_DELAY",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },
              { "scFollowActor", new OASISCommand {opCode="SC_FOLLOW_ACTOR",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },
              { "scSetAnimstate", new OASISCommand {opCode="SC_SET_ANIMSTATE",nArguments=2, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "scPanCamera", new OASISCommand {opCode="SC_PAN_CAMERA",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },
              { "scWaitForCamera", new OASISCommand{opCode="SC_WAIT_FOR_CAMERA", nArguments=0, paramList= new List<Symbol.Types> { } } },
              { "scLoadRoom", new OASISCommand {opCode="SC_LOAD_ROOM",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },
              { "scSetEgo", new OASISCommand {opCode="SC_SET_EGO",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },
              { "scBreakHere", new OASISCommand{opCode="SC_BREAK_HERE", nArguments=0, paramList= new List<Symbol.Types> { } } },
              { "scSetPosition", new OASISCommand {opCode="SC_SET_OBJECT_POS",nArguments=4, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte, Symbol.Types.Byte, Symbol.Types.Byte } } },

              // Temporal!!
              { "scChangeRoomAndStop", new OASISCommand {opCode="SC_CHANGE_ROOM_AND_STOP",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },
              // SC_ASSIGN		16 and SC_SETFLAG		17 are used automatically by the numeric asingment and logical counterpart evaluators

              { "scExecuteAction", new OASISCommand {opCode="SC_EXECUTE_ACTION",nArguments=4, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte, Symbol.Types.Byte, Symbol.Types.Byte } } },

              //SC_JUMP_REL	19 and SC_JUMP_REL_IF 20 are used by the compiler

              { "scChainScript", new OASISCommand {opCode="SC_CHAIN_SCRIPT",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },
              { "scSpawnScript", new OASISCommand {opCode="SC_SPAWN_SCRIPT",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },
              { "scSetEvents", new OASISCommand {opCode="SC_SET_EVENTS",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },
              { "scClearEvents", new OASISCommand {opCode="SC_CLEAR_EVENTS",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },
              { "scRunObjectCode", new OASISCommand {opCode="SC_RUN_OBJECT_CODE",nArguments=3, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "scSetCameraAt", new OASISCommand {opCode="SC_SET_CAMERA_AT",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },
              { "scSetFadeEffect", new OASISCommand {opCode="SC_SET_FADEEFFECT",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },
              { "scCursorOn", new OASISCommand{opCode="SC_CURSOR_ON", nArguments=1, paramList= new List<Symbol.Types> { Symbol.Types.Bool} } },
              { "scLookDirection", new OASISCommand {opCode="SC_LOOK_DIRECTION",nArguments=2, paramList= new List<Symbol.Types>{  Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "scSetState", new OASISCommand {opCode="SC_SET_STATE",nArguments=2, paramList= new List<Symbol.Types>{  Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "scSetCostume", new OASISCommand {opCode="SC_SET_COSTUME",nArguments=3, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "scDisableVerb", new OASISCommand {opCode="SC_DISABLE_VERB",nArguments=2, paramList= new List<Symbol.Types>{  Symbol.Types.Byte, Symbol.Types.Bool} } },
              { "scSetWalkboxAsWalkable", new OASISCommand {opCode="SC_SET_WBASWALKABLE",nArguments=2, paramList= new List<Symbol.Types>{  Symbol.Types.Byte, Symbol.Types.Bool} } },
              { "scSetNextWalkbox", new OASISCommand {opCode="SC_SET_NEXTWB",nArguments=3, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "scPlayTune", new OASISCommand {opCode="SC_PLAY_TUNE",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },
              { "scWaitForTune", new OASISCommand{opCode="SC_WAIT_FOR_TUNE", nArguments=0, paramList= new List<Symbol.Types> { } } },
              { "scStopTune", new OASISCommand{opCode="SC_STOP_TUNE", nArguments=0, paramList= new List<Symbol.Types> { } } },
              { "scShowVerbs", new OASISCommand{opCode="SC_SHOW_VERBS", nArguments=1, paramList= new List<Symbol.Types> { Symbol.Types.Bool } } },
              { "scPrint", new OASISCommand {opCode="SC_PRINT",nArguments=2, paramList= new List<Symbol.Types>{  Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "scPrintAt", new OASISCommand {opCode="SC_PRINT_AT",nArguments=4, paramList= new List<Symbol.Types>{  Symbol.Types.Byte, Symbol.Types.Byte, Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "scRedrawScreen", new OASISCommand{opCode="SC_REDRAW_SCREEN", nArguments=0, paramList= new List<Symbol.Types> { } } },
              { "scPutInInventory", new OASISCommand {opCode="SC_PUT_IN_INVENTORY",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },
              { "scRemoveFromInventory", new OASISCommand {opCode="SC_REMOVE_FROM_INVENTORY",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },
              { "scLoadResource", new OASISCommand {opCode="SC_LOAD_RESOURCE",nArguments=2, paramList= new List<Symbol.Types>{  Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "scNukeResource", new OASISCommand {opCode="SC_NUKE_RESOURCE",nArguments=2, paramList= new List<Symbol.Types>{  Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "scLockResource", new OASISCommand {opCode="SC_LOCK_RESOURCE",nArguments=2, paramList= new List<Symbol.Types>{  Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "scUnlockResource", new OASISCommand {opCode="SC_UNLOCK_RESOURCE",nArguments=2, paramList= new List<Symbol.Types>{  Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "scLoadObjectToGame", new OASISCommand {opCode="SC_LOAD_OBJECT",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },
              { "scRemoveObjectFromGame", new OASISCommand {opCode="SC_REMOVE_OBJECT",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Byte } } },

              { "scSetOverrideJump", new OASISCommand {opCode="SC_SET_OVERRIDE",nArguments=1, paramList= new List<Symbol.Types>{  Symbol.Types.Word } } },

              { "scClearRoomArea", new OASISCommand{opCode="SC_CLEAR_ROOMAREA", nArguments=0, paramList= new List<Symbol.Types> { } } },

              // SC_JUMP 52 and SC_JUMP_IF 53 are used by the compiler

              { "scStartDialog", new OASISCommand{opCode="SC_START_DIALOG", nArguments=0, paramList= new List<Symbol.Types> { } } },
              { "scEndDialog", new OASISCommand{opCode="SC_END_DIALOG", nArguments=0, paramList= new List<Symbol.Types> { } } },
              { "scLoadDialog", new OASISCommand{opCode="SC_LOAD_DIALOG", nArguments=1, paramList= new List<Symbol.Types> { Symbol.Types.Byte } } },
              { "scActivateDlgOption", new OASISCommand{opCode="SC_ACTIVATE_DLGOPT", nArguments=2, paramList= new List<Symbol.Types> { Symbol.Types.Byte, Symbol.Types.Bool } } },

              { "scFreezeScript", new OASISCommand{opCode="SC_FREEZE_SCRIPT", nArguments=2, paramList= new List<Symbol.Types> { Symbol.Types.Byte, Symbol.Types.Bool } } },
              { "scFreezeAllScripts", new OASISCommand{opCode="SC_FREEZE_ALL_SCRIPTS", nArguments=1, paramList= new List<Symbol.Types> { Symbol.Types.Bool } } },
              { "scTerminateScript", new OASISCommand{opCode="SC_TERMINATE_SCRIPT", nArguments=1, paramList= new List<Symbol.Types> { Symbol.Types.Byte } } },


          };

        }


    }
    }



