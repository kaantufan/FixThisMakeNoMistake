using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TooLateToPanic
{
    // JsonUtility coerces null rows and wrong numeric types to default objects/zero.
    // Preserve optional legacy fields while checking the declared types of present score fields.
    internal static class PanicScoreShape
    {
        internal static bool TryEntries(string json,out List<bool> entries)
        {
            entries=null;
            try
            {
                var r=new Reader(json);r.Expect('{');r.Space();
                if(r.Take('}'))return false;
                do
                {
                    string key=r.String();r.Expect(':');
                    if(key=="entries")entries=r.Entries();else if(key=="schema"){if(!r.SchemaType())return false;}else r.Skip(0);
                }while(r.Take(','));
                r.Expect('}');r.Space();return entries!=null&&r.End;
            }
            catch(FormatException){return false;}
        }
        sealed class Reader
        {
            readonly string s;int p;
            internal Reader(string text){s=text??"";}
            internal bool End=>p==s.Length;
            internal void Space(){while(p<s.Length&&(s[p]==' '||s[p]=='\r'||s[p]=='\n'||s[p]=='\t'))p++;}
            internal bool Take(char c){Space();if(p<s.Length&&s[p]==c){p++;return true;}return false;}
            internal void Expect(char c){if(!Take(c))throw new FormatException();}
            internal string String()
            {
                Expect('"');var value=new StringBuilder();
                while(p<s.Length)
                {
                    char c=s[p++];if(c=='"')return value.ToString();if(c<32)throw new FormatException();
                    if(c!='\\'){value.Append(c);continue;}
                    if(p>=s.Length)throw new FormatException();c=s[p++];
                    switch(c)
                    {
                        case '"':case '\\':case '/':value.Append(c);break;
                        case 'b':value.Append('\b');break;case 'f':value.Append('\f');break;case 'n':value.Append('\n');break;case 'r':value.Append('\r');break;case 't':value.Append('\t');break;
                        case 'u':if(p+4>s.Length||!ushort.TryParse(s.Substring(p,4),NumberStyles.AllowHexSpecifier,CultureInfo.InvariantCulture,out ushort code))throw new FormatException();value.Append((char)code);p+=4;break;
                        default:throw new FormatException();
                    }
                }
                throw new FormatException();
            }
            internal List<bool> Entries()
            {
                Expect('[');var result=new List<bool>();if(Take(']'))return result;
                do{Space();if(p<s.Length&&s[p]=='{')result.Add(Row());else{Skip(0);result.Add(false);}}while(Take(','));
                Expect(']');return result;
            }
            bool Row()
            {
                Expect('{');bool valid=true;if(Take('}'))return true;
                do
                {
                    string key=String();Expect(':');Space();int start=p;Skip(0);int end=p;
                    if(key=="waves"||key=="score")
                    {
                        string token=s.Substring(start,end-start);valid&=IntegerToken(token);
                        if(key=="waves")valid&=int.TryParse(token,NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture,out _);
                        else valid&=long.TryParse(token,NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture,out _);
                    }
                    else if(key=="date")valid&=start<end&&(s[start]=='"'||s.Substring(start,end-start)=="null");
                }while(Take(','));
                Expect('}');return valid;
            }
            static bool IntegerToken(string value)
            {
                int i=value.StartsWith("-",StringComparison.Ordinal)?1:0;if(i==value.Length)return false;
                if(value[i]=='0'&&i+1<value.Length)return false;
                for(;i<value.Length;i++)if(value[i]<'0'||value[i]>'9')return false;return true;
            }
            internal bool SchemaType(){Space();int start=p;Skip(0);string token=s.Substring(start,p-start);return IntegerToken(token)&&int.TryParse(token,NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture,out _);}
            internal void Skip(int depth)
            {
                if(depth>32)throw new FormatException();Space();if(p>=s.Length)throw new FormatException();
                if(s[p]=='"'){String();return;}
                if(Take('{')){if(Take('}'))return;do{String();Expect(':');Skip(depth+1);}while(Take(','));Expect('}');return;}
                if(Take('[')){if(Take(']'))return;do{Skip(depth+1);}while(Take(','));Expect(']');return;}
                int start=p;while(p<s.Length&&s[p]!=','&&s[p]!='}'&&s[p]!=']'&&!char.IsWhiteSpace(s[p]))p++;
                if(start==p)throw new FormatException();
            }
        }
    }
}
