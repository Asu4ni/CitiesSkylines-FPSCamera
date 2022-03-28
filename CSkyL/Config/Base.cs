namespace CSkyL.Config
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Serialization;

    public abstract class Base : IXmlSerializable
    {
        protected Base(string filePath)
        {
            _filePath = filePath;
        }

        public virtual void Assign(Base other)
        {
            if (GetType() != other.GetType()) {
                Log.Warn($"Config: cannot assign <{other.GetType().Name}> to <{GetType().Name}>");
                return;
            }
            foreach (var field in GetType().GetFields(
                                       BindingFlags.Public | BindingFlags.Instance)) {
                if (field.GetValue(this) is IConfigData cur &&
                    field.GetValue(other) is IConfigData oth) cur.Assign(oth);
            }
        }
        public virtual void Reset()
            => Assign(System.Activator.CreateInstance(GetType()) as Base);

        /*--------- serialization -------------------------------------------*/

        public void Save(object _ = null) { Save(this); }
        public static void Save(Base config, string path = null)
        {
            if (path is null) path = config._filePath;

            var serializer = new XmlSerializer(config.GetType());
            using (var writer = new StreamWriter(path)) {
                serializer.Serialize(writer, config);
            }
        }

        public static TConfig Load<TConfig>(string path)
                                       where TConfig : Base
        {

            var serializer = new XmlSerializer(typeof(TConfig));
            try {
                using (var reader = new StreamReader(path)) {
                    return (TConfig) serializer.Deserialize(reader);
                }
            }
            catch (FileNotFoundException) {
                Log.Msg($"Config: file ({path}) missing");
            }
            catch (System.Exception e) {
                Log.Err($"Config: exception while loading ({path}): {e}");
            }
            return null;
        }


        public System.Xml.Schema.XmlSchema GetSchema() => null;
        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            while (reader.IsStartElement()) {
                if (reader.IsEmptyElement) {
                    reader.ReadStartElement();
                    continue;
                }
                var fieldName = reader.Name;
                reader.ReadStartElement();
                if (GetType().GetField(fieldName) is FieldInfo field) {
                    if (field.GetValue(this) is IConfigData config) {
                        var str = reader.ReadContentAsString();
                        if (!config.AssignByParsing(str))
                            Log.Warn($"Config: invalid value({str}) for field[{fieldName}]");
                    }
                    else Log.Err($"Config: invalid type of config field[{fieldName}]");
                }
                else {
                    field = GetType().GetField(fieldName, BindingFlags.NonPublic |
                                                          BindingFlags.Instance);
                    if (field is null || !_ReadXmlForPrivate(field.GetValue(this), reader)) {
                        Log.Warn($"Config: unknown config field name [{fieldName}]");
                        reader.Skip();
                    }
                }
                reader.ReadEndElement();
            }
            reader.ReadEndElement();
        }
        public void WriteXml(XmlWriter writer)
        {
            foreach (var field in GetType().GetFields(BindingFlags.Public |
                                                      BindingFlags.Instance)) {
                if (field.GetValue(this) is IConfigData config) {
                    writer.WriteStartElement(field.Name);
                    writer.WriteString(config.ToString());
                    writer.WriteEndElement();
                }
                else Log.Err($"Config: invalid type of config field[{field.Name}]");
            }
            foreach (var field in GetType().GetFields(BindingFlags.NonPublic |
                                                      BindingFlags.Instance)) {
                if (field.GetValue(this) is object obj)
                    _WriteXmlForPrivate(field.Name, obj, writer);
            }
        }

        // return false if reading <field> is not supported
        protected virtual bool _ReadXmlForPrivate(object field, XmlReader reader)
        {
            // dictionary of string keys and IConfigData value
            if (field.GetType() is Type t && t.IsGenericType &&
                    t.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
                    t.GetGenericArguments()[0] == typeof(string) &&
                    typeof(IConfigData).IsAssignableFrom(t.GetGenericArguments()[1])) {
                var method = GetType().GetMethod("_ReadDictXml",
                                           BindingFlags.NonPublic | BindingFlags.Instance)
                                      .MakeGenericMethod(t.GetGenericArguments()[1]);
                method.Invoke(this, new object[] { field, reader });
                return true;
            }
            return false;
        }
        protected virtual void _ReadDictXml<TConfig>(
                Dictionary<string, TConfig> dictField, XmlReader reader)
                where TConfig : IConfigData, new()
        {
            while (reader.IsStartElement()) {
                var key = reader.Name;
                reader.ReadStartElement();
                var str = reader.ReadContentAsString();
                var config = _DefaultFor<TConfig>();
                if (config.AssignByParsing(str)) {
                    dictField[key] = config;
                }
                else Log.Warn($"Config: invalid value({str}) for a <{typeof(TConfig).Name}>");
                reader.ReadEndElement();
            }
        }
        protected virtual TConfig _DefaultFor<TConfig>() where TConfig : IConfigData
            => default;

        // return false if reading <field> is not supported
        protected virtual bool _WriteXmlForPrivate(string name, object field, XmlWriter writer)
        {
            // dictionary of string keys and IConfigData value
            if (field.GetType() is Type t && t.IsGenericType &&
                    t.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
                    t.GetGenericArguments()[0] == typeof(string) &&
                    typeof(IConfigData).IsAssignableFrom(t.GetGenericArguments()[1])) {
                var method = GetType().GetMethod("_WriteDictXml",
                                           BindingFlags.NonPublic | BindingFlags.Instance)
                                      .MakeGenericMethod(t.GetGenericArguments()[1]);
                writer.WriteStartElement(name);
                method.Invoke(this, new object[] { field, writer });
                writer.WriteEndElement();
                return true;
            }
            return false;
        }
        protected virtual void _WriteDictXml<TConfig>(
                Dictionary<string, TConfig> dictField, XmlWriter writer)
                where TConfig : IConfigData, new()
        {
            foreach (var config in dictField) {
                writer.WriteStartElement(config.Key);
                writer.WriteString(config.Value.ToString());
                writer.WriteEndElement();
            }
        }

        private readonly string _filePath;
    }
}
