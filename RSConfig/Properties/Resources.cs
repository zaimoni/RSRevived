// Decompiled with JetBrains decompiler
// Type: Setup.Properties.Resources
// Assembly: RSConfig, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A6245E6-7D9A-4424-BC16-B17B9A5036B9
// Assembly location: C:\Private.app\RS9Alpha.Hg\RSConfig.exe

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Setup.Properties
{
  [DebuggerNonUserCode]
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  [CompilerGenerated]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (object.ReferenceEquals((object) Setup.Properties.Resources.resourceMan, (object) null))
          Setup.Properties.Resources.resourceMan = new ResourceManager("Setup.Properties.Resources", typeof (Setup.Properties.Resources).Assembly);
        return Setup.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get
      {
        return Setup.Properties.Resources.resourceCulture;
      }
      set
      {
        Setup.Properties.Resources.resourceCulture = value;
      }
    }

    internal Resources()
    {
    }
  }
}
