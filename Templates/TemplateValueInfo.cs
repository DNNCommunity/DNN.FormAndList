using System;
using System.Xml;

namespace DotNetNuke.Modules.UserDefinedTable.Templates
{
    [Serializable]
    public class TemplateValueInfo
    {
        #region Private Fields

        int _EditorId;
        string _ValueSource;
        string _Value;
        string _Caption;
        XmlNode _Node;
        int _length;

        #endregion

        #region Public Properties

        public int Editor
        {
            get { return _EditorId; }
            set { _EditorId = value; }
        }

        public string ValueSource
        {
            get { return _ValueSource; }
            set { _ValueSource = value; }
        }

        public string Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        public string Caption
        {
            get { return _Caption; }
            set { _Caption = value; }
        }

        public XmlNode Node
        {
            get { return _Node; }
            set { _Node = value; }
        }

        public bool TrueColumn
        {
            get { return true; }
        }

        public bool FalseColumn
        {
            get { return false; }
        }

        public int Length
        {
            get { return _length; }
            set { _length = value; }
        }

        #endregion
    }
}