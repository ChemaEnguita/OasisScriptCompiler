using System.Collections.Generic;

namespace OASISCompiler
{
    class OASISFunction
    {
        public string opCode; // string as used in #define
        public Symbol.Types returnType;
        public int nArguments;
        public List<Symbol.Types> paramList;
    }

    class OASISFunctions
    {
        Dictionary<string, OASISFunction> functions;

        public OASISFunction resolve(string name)
        {
            OASISFunction f;
            if (functions.TryGetValue(name, out f))
                return f;
            else
                return null;
        }


        public OASISFunctions()
        {
          functions = new Dictionary<string, OASISFunction>()
          {
              /* These are automatically used by the expression evaluator
              { "sfAdd", new OASISFunction {opCode="SF_ADD",  returnType=Symbol.Types.Byte, nArguments=2, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "sfSub", new OASISFunction {opCode="SF_SUB",  returnType=Symbol.Types.Byte, nArguments=2, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "sfMul", new OASISFunction {opCode="SF_MUL",  returnType=Symbol.Types.Byte, nArguments=2, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "sfDiv", new OASISFunction {opCode="SF_DIV",  returnType=Symbol.Types.Byte, nArguments=2, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte } } },

              { "sfEqual", new OASISFunction {opCode="SF_EQ",  returnType=Symbol.Types.Bool, nArguments=2, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "sfGT", new OASISFunction {opCode="SF_GT",  returnType=Symbol.Types.Bool, nArguments=2, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "sfGE", new OASISFunction {opCode="SF_GE",  returnType=Symbol.Types.Bool, nArguments=2, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "sfLT", new OASISFunction {opCode="SF_LT",  returnType=Symbol.Types.Bool, nArguments=2, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "sfLE", new OASISFunction {opCode="SF_LE",  returnType=Symbol.Types.Bool, nArguments=2, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte } } },

              { "sfAnd", new OASISFunction {opCode="SF_AND",  returnType=Symbol.Types.Bool, nArguments=2, paramList= new List<Symbol.Types>{ Symbol.Types.Bool, Symbol.Types.Bool } } },
              { "sfOr", new OASISFunction {opCode="SF_OR",  returnType=Symbol.Types.Bool, nArguments=2, paramList= new List<Symbol.Types>{ Symbol.Types.Bool, Symbol.Types.Bool } } },
              //{ "sfXor", new OASISFunction {opCode="SF_OR",  returnType=Symbol.Types.Bool, nArguments=2, paramList= new List<Symbol.Types>{ Symbol.Types.Bool, Symbol.Types.Bool } } },
              { "sfNot", new OASISFunction {opCode="SF_NOT",  returnType=Symbol.Types.Bool, nArguments=1, paramList= new List<Symbol.Types>{Symbol.Types.Bool } } },
              { "sfGetVal", new OASISFunction {opCode="SF_GETVAL",  returnType=Symbol.Types.Byte, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte } } },
              { "sfGetFlag", new OASISFunction {opCode="SF_GETFLAG",  returnType=Symbol.Types.Bool, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte } } },
               */

              { "sfGetRand", new OASISFunction {opCode="SF_GETRAND",  returnType=Symbol.Types.Byte, nArguments=0, paramList= new List<Symbol.Types> { } } },
              { "sfGetRandInt", new OASISFunction {opCode="SF_GETRANDINT",  returnType=Symbol.Types.Byte, nArguments=2, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte } } },

              { "sfGetEgo", new OASISFunction {opCode="SF_GET_EGO",  returnType=Symbol.Types.Byte, nArguments=0, paramList= new List<Symbol.Types>{ } } },
              { "sfGetTalking", new OASISFunction {opCode="SF_GET_TALKING",  returnType=Symbol.Types.Byte, nArguments=0, paramList= new List<Symbol.Types>{ } } },
              { "sfGetRow", new OASISFunction {opCode="SF_GET_COL",  returnType=Symbol.Types.Byte, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },
              { "sfGetRoom", new OASISFunction {opCode="SF_GET_ROOM",  returnType=Symbol.Types.Byte, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },
              { "sfGetWalkbox", new OASISFunction {opCode="SF_GET_WALKBOX",  returnType=Symbol.Types.Byte, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },
              { "sfGetFacing", new OASISFunction {opCode="SF_GET_FACING",  returnType=Symbol.Types.Byte, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },
              { "sfGetCostumeID", new OASISFunction {opCode="SF_GET_COSTID",  returnType=Symbol.Types.Byte, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },
              { "sfGetCostumeNo", new OASISFunction {opCode="SF_GET_COSTNO",  returnType=Symbol.Types.Byte, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },
              { "sfGetAnimstate", new OASISFunction {opCode="SF_GET_ANIMSTATE",  returnType=Symbol.Types.Byte, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },

              { "sfGetState", new OASISFunction {opCode="SF_GET_STATE",  returnType=Symbol.Types.Byte, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },

              { "sfGetWalkRow", new OASISFunction {opCode="SF_GET_WALKROW",  returnType=Symbol.Types.Byte, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },
              { "sfGetWalkCol", new OASISFunction {opCode="SF_GET_WALKCOL",  returnType=Symbol.Types.Byte, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },
              { "sfGetWalkFaceDir", new OASISFunction {opCode="SF_GET_WALKFACEDIR",  returnType=Symbol.Types.Byte, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },
              { "sfGetColorSpeech", new OASISFunction {opCode="SF_GET_COLORSPEECH",  returnType=Symbol.Types.Byte, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },
              { "sfGetSizeX", new OASISFunction {opCode="SF_GET_SIZEX",  returnType=Symbol.Types.Byte, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },
              { "sfGetSizeY", new OASISFunction {opCode="SF_GET_SIZEY",  returnType=Symbol.Types.Byte, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },
              { "sfGetAnimSpeed", new OASISFunction {opCode="SF_GET_ANIMSPEED",  returnType=Symbol.Types.Byte, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },
              { "sfGetIsActor", new OASISFunction {opCode="SF_IS_ACTOR",  returnType=Symbol.Types.Bool, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },
              { "sfIsProp", new OASISFunction {opCode="SF_IS_PROP",  returnType=Symbol.Types.Bool, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },
              { "sfIsNotMoving", new OASISFunction {opCode="SF_IS_NOTMOVING",  returnType=Symbol.Types.Bool, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },
              { "sfGetClosestActor", new OASISFunction {opCode="SF_GET_CLOSESTACTOR",  returnType=Symbol.Types.Byte, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },
              { "sfGetDistance", new OASISFunction {opCode="SF_GET_DISTANCE",  returnType=Symbol.Types.Byte, nArguments=2, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "sfGetObjectAt", new OASISFunction {opCode="SF_GET_OBJAT",  returnType=Symbol.Types.Byte, nArguments=2, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "sfISObjectInInventory", new OASISFunction {opCode="SF_IS_OBJINVENTORY",  returnType=Symbol.Types.Bool, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte} } },

              { "sfGetCameraCol", new OASISFunction {opCode="SF_GET_CAMERACOL",  returnType=Symbol.Types.Byte, nArguments=0, paramList= new List<Symbol.Types>{ } } },
              { "sfGetCameraFollowing", new OASISFunction {opCode="SF_GET_CAMERAFOLLOWING",  returnType=Symbol.Types.Byte, nArguments=0, paramList= new List<Symbol.Types>{ } } },
              { "sfGetFadeEffect", new OASISFunction {opCode="SF_GET_FADEEFFECT",  returnType=Symbol.Types.Byte, nArguments=0, paramList= new List<Symbol.Types>{ } } },
              { "sfIsCameraInAction", new OASISFunction {opCode="SF_IS_CAMERAINACTION",  returnType=Symbol.Types.Bool, nArguments=0, paramList= new List<Symbol.Types>{ } } },

              { "sfGetCurrentRoom", new OASISFunction {opCode="SF_GET_CURROOM",  returnType=Symbol.Types.Byte, nArguments=0, paramList= new List<Symbol.Types>{ } } },
              { "sfGetRoomCols", new OASISFunction {opCode="SF_GET_ROOMCOLS",  returnType=Symbol.Types.Byte, nArguments=0, paramList= new List<Symbol.Types>{ } } },

              { "sfIsScriptRunning", new OASISFunction {opCode="SF_IS_SCRIPTRUNNING",  returnType=Symbol.Types.Bool, nArguments=2, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte } } },
              { "sfIsMusicPlaying", new OASISFunction {opCode="SF_IS_MUSICPLAYING",  returnType=Symbol.Types.Bool, nArguments=0, paramList= new List<Symbol.Types>{ } } },
              { "sfIsWalkboxWalkable", new OASISFunction {opCode="SF_IS_WALKBOXWALKABLE",  returnType=Symbol.Types.Bool, nArguments=1, paramList= new List<Symbol.Types>{ Symbol.Types.Byte } } },
              { "sfGetNextWalkbox", new OASISFunction {opCode="SF_GET_NEXTWALKBOX",  returnType=Symbol.Types.Byte, nArguments=2, paramList= new List<Symbol.Types>{ Symbol.Types.Byte, Symbol.Types.Byte } } },

              { "sfGetActorExecutingAction", new OASISFunction {opCode="SF_GET_ACTIONACTOR",  returnType=Symbol.Types.Byte, nArguments=0, paramList= new List<Symbol.Types>{ } } },
              { "sfGetActionVerb", new OASISFunction {opCode="SF_GET_ACTIONVERB",  returnType=Symbol.Types.Byte, nArguments=0, paramList= new List<Symbol.Types>{ } } },
              { "sfGetActionObject1", new OASISFunction {opCode="SF_GET_ACTIONOBJ1",  returnType=Symbol.Types.Byte, nArguments=0, paramList= new List<Symbol.Types>{ } } },
              { "sfGetActionObject2", new OASISFunction {opCode="SF_GET_ACTIONOBJ2",  returnType=Symbol.Types.Byte, nArguments=0, paramList= new List<Symbol.Types>{ } } },
  
          };

        }


    }
}
