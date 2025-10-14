// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.World
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D<short>;
using Rectangle = Zaimoni.Data.Box2D<short>;

#nullable enable

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  public sealed class World : Zaimoni.Serialization.ISerialize
  {
    // alpha10
    // weather stays from 1h to 3 days and then change
    private const int WEATHER_MIN_DURATION = 1 * WorldTime.TURNS_PER_HOUR;
    private const int WEATHER_MAX_DURATION = 3 * WorldTime.TURNS_PER_DAY;

    private static World? s_Recent = null; // most recently constructed World; our owner Session is a Singleton
    public static World Get { get { return s_Recent ?? throw new ArgumentNullException(nameof(s_Recent)); } }

    // VAPORWARE: non-city districts outside of city limits (both gas station and National Guard base will be outside city limits)
    static public readonly Point CHAR_City_Origin = new Point(1, 1);
    [NonSerialized] private Rectangle m_CHAR_City;
    public Rectangle CHAR_CityLimits { get { return m_CHAR_City; } }

    private readonly District[,] m_DistrictsGrid;
    private readonly short m_Size;
    [NonSerialized] private Rectangle m_Extent;
    public Rectangle Extent { get { return m_Extent; } }

    private District? m_PlayerDistrict = null;
    private District? m_SimDistrict = null;
    private readonly List<District> m_Ready = new();   // \todo this is expected to have a small maximum that can be hard-coded; measure it
    public Weather Weather { get; private set; }
    public int NextWeatherCheckTurn { get; private set; } // alpha10

    public short Size { get { return m_Size; } }
    public short CitySize { get { return m_CHAR_City.Width; } }  // not guaranteed to be the same as the above

    public bool InBounds(int x,int y) {
      return 0 <= x && m_Size > x && 0 <= y && m_Size > y;
    }
    public bool InBounds(Point pt) {
      return 0 <= pt.X && m_Size > pt.X && 0 <= pt.Y && m_Size > pt.Y;
    }
    public District? At(int x, int y) { return InBounds(x, y) ? m_DistrictsGrid[x, y] : null; }
    public District? At(Point pt) { return InBounds(pt) ? m_DistrictsGrid[pt.X, pt.Y] : null; }

    public District this[int x, int y]
    {
      get {
#if DEBUG
        if (!InBounds(x, y)) throw new InvalidOperationException("not in bounds");
#endif
        return m_DistrictsGrid[x, y];
      }
      set {
#if DEBUG
        if (!InBounds(x, y)) throw new InvalidOperationException("not in bounds");
#endif
        m_DistrictsGrid[x, y] = value;
      }
    }

    public District this[Point pt]
    {
      get {
#if DEBUG
        if (!InBounds(pt)) throw new InvalidOperationException("not in bounds");
#endif
        return m_DistrictsGrid[pt.X, pt.Y];
      }
      set {
#if DEBUG
        if (!InBounds(pt)) throw new InvalidOperationException("not in bounds");
#endif
        m_DistrictsGrid[pt.X, pt.Y] = value;
      }
    }

    // unsure that city/game world will be a square of districts indefinitely so use this wrapper
    /// <returns>The last district in the turn sequencing order</returns>
    public District Last { get { return m_DistrictsGrid[m_Size - 1, m_Size - 1]; } }
    /// <returns>district is on east edge of world</returns>
    public bool Edge_E(District d) { return d.WorldPosition.X == m_Size - 1; }
    /// <returns>district is on south edge of world</returns>
    public bool Edge_S(District d) { return d.WorldPosition.Y == m_Size - 1; }
    /// <returns>district is on north or east edge of world</returns>
    static public bool Edge_N_or_W(District d) { return 0 >= d.WorldPosition.X || 0 >= d.WorldPosition.Y; }    // arguably should be 0 ==

    public uint EdgeCode(District d) {
        uint ret = 0;
        if (Extent.Top >= d.WorldPosition.Y) ret |= 1;
        if (Extent.Right - 1 <= d.WorldPosition.X) ret |= 2;
        if (Extent.Bottom - 1 <= d.WorldPosition.Y) ret |= 4;
        if (Extent.Left >= d.WorldPosition.X) ret |= 8;
	    return ret;
    }

    // static bool WithinCityLimits(Point pos) { return true; }  // VAPORWARE
    // supported layouts are: E-W, N-S, 
    // the four Ts with 3 cardinal directions linked to neutral (represented as two line segments)
    // 4-way interesection (represented as two line segments, E-W and N-S)
    // diagonals: S-E, S-W, N-E, N-W
    public uint SubwayLayout(Point pos)
    {
      if (40 > Engine.RogueGame.Options.DistrictSize) return 0; // 30 is known to be impossible to get a subway station.  40 is ok
      if (!CHAR_CityLimits.Contains(pos)) return 0; // no subway outside of city limits.

      // precompute some line segments
      const uint E_W = (uint)Compass.XCOMlike.E * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.W;
      const uint N_S = (uint)Compass.XCOMlike.N * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.S;
      const uint N_E = (uint)Compass.XCOMlike.N * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.E;
      const uint N_W = (uint)Compass.XCOMlike.N * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.W;
      const uint S_E = (uint)Compass.XCOMlike.E * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.S;
      const uint S_W = (uint)Compass.XCOMlike.S * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.W;
      const uint N_NEUTRAL = (uint)Compass.XCOMlike.N * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      const uint E_NEUTRAL = (uint)Compass.XCOMlike.E * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      const uint S_NEUTRAL = (uint)Compass.XCOMlike.S * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      const uint W_NEUTRAL = (uint)Compass.XCOMlike.W * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      const uint FOUR_WAY = N_S * (uint)Compass.reference.XCOM_LINE_SEGMENT_UB + E_W;
      const uint N_TEE = N_NEUTRAL * (uint)Compass.reference.XCOM_LINE_SEGMENT_UB + E_W;
      const uint S_TEE = S_NEUTRAL * (uint)Compass.reference.XCOM_LINE_SEGMENT_UB + E_W;
      const uint E_TEE = N_S * (uint)Compass.reference.XCOM_LINE_SEGMENT_UB + E_NEUTRAL;
      const uint W_TEE = N_S * (uint)Compass.reference.XCOM_LINE_SEGMENT_UB + W_NEUTRAL;

      // map generation, so doesn't have to be fast
      var tl_city = CHAR_CityLimits.Location;
      var br_city = CHAR_CityLimits.Location + CHAR_CityLimits.Size + Direction.NW;
      var mid_city = CHAR_CityLimits.Location + CHAR_CityLimits.Size / 2;

      if (tl_city.Y == pos.Y) {
        if (tl_city.X == pos.X) return S_E;
        else if (mid_city.X == pos.X) return S_TEE;
        else if (br_city.X == pos.X) return S_W;
        else return E_W;
      } else if (mid_city.Y == pos.Y) {
        if (tl_city.X == pos.X) return E_TEE;
        else if (mid_city.X == pos.X) return FOUR_WAY;
        else if (br_city.X == pos.X) return W_TEE;
        else return E_W;
      } else if (br_city.Y == pos.Y) {
        if (tl_city.X == pos.X) return N_E;
        else if (mid_city.X == pos.X) return N_TEE;
        else if (br_city.X == pos.X) return N_W;
        else return E_W;
      } else if (tl_city.X == pos.X) return N_S;
      else if (mid_city.X == pos.X) return N_S;
      else if (br_city.X == pos.X) return N_S;
      return 0; // any valid layout will have at least one line segment and thus be non-zero
    }

    public uint HighwayLayout(Point pos)
    {
      // precompute some line segments (must agree with BaseTownGenerator::NewSurfaceBlocks)
      const uint E_W = (uint)Compass.XCOMlike.E * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.W;
      const uint N_S = (uint)Compass.XCOMlike.N * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.S;
      const uint N_E = (uint)Compass.XCOMlike.N * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.E;
      const uint N_W = (uint)Compass.XCOMlike.N * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.W;
      const uint S_E = (uint)Compass.XCOMlike.E * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.S;
      const uint S_W = (uint)Compass.XCOMlike.S * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.W;
      const uint FOUR_WAY = N_S * (uint)Compass.reference.XCOM_LINE_SEGMENT_UB + E_W;  // not quite right but we can counter-adjust later

      // map generation, so doesn't have to be fast
      var tl_highway = CHAR_CityLimits.Location + Direction.NW;
      var br_highway = CHAR_CityLimits.Location + CHAR_CityLimits.Size;
      var mid_highway = CHAR_CityLimits.Location + CHAR_CityLimits.Size / 2;

      if (tl_highway.Y == pos.Y) {
        if (tl_highway.X == pos.X) return S_E;
        else if (br_highway.X == pos.X) return S_W;
        else if (mid_highway.X == pos.X) return FOUR_WAY;
        else return E_W;
      } else if (br_highway.Y == pos.Y) {
        if (tl_highway.X == pos.X) return N_E;
        else if (br_highway.X == pos.X) return N_W;
        else if (mid_highway.X == pos.X) return FOUR_WAY;
        else return E_W;
      } else if (tl_highway.X == pos.X) return (mid_highway.Y == pos.Y) ? FOUR_WAY : N_S;
      else if (br_highway.X == pos.X) return (mid_highway.Y == pos.Y) ? FOUR_WAY : N_S;
      return 0; // any valid layout will have at least one line segment and thus be non-zero
    }

    // cannot return IEnumerable<District>, but this does not error
    public void DoForAllDistricts(Action<District> op)
    {
      foreach(District d in m_DistrictsGrid) op(d);
    }

    public void DoForAllDistricts(Action<World,District> op)
    {
      foreach(District d in m_DistrictsGrid) op(this, d);
    }

    public void DoForAllMaps(Action<Map> op,Predicate<District> ok)
    {
      foreach(District d in m_DistrictsGrid) {
        if (null != ok && !ok(d)) continue;
        foreach(Map m in d.Maps) op(m);
      }
    }

    public void DoForAllMaps(Action<Map> op)
    {
      foreach(District d in m_DistrictsGrid) {
        foreach(Map m in d.Maps) op(m);
      }
    }

    public bool Any(Predicate<Map> test)
    {
      foreach(District d in m_DistrictsGrid) {
        foreach(Map m in d.Maps) if (test(m)) return true;
      }
      return false;
    }

    public Actor? From(in ActorTag src) {
        foreach(District d in m_DistrictsGrid) {
            var actor = d.From(in src);
            if (null != actor) return actor;
        }
        return null;
    }

    public Corpse? CorpseFrom(in ActorTag src) {
        foreach(District d in m_DistrictsGrid) {
            var c = d.CorpseFrom(in src);
            if (null != c) return c;
        }
        return null;
    }

    public void DoForAllActors(Action<Actor> op) { foreach(District d in m_DistrictsGrid) d.DoForAllActors(op); }
    public void DoForAllActors(Predicate<Map> ok, Action<Actor> op) { foreach(District d in m_DistrictsGrid) d.DoForAllActors(ok, op); }
    public void DoForAllGroundInventories(Action<Location,Inventory> op) { foreach (District d in m_DistrictsGrid) d.DoForAllGroundInventories(op); }

    public bool WantToAdvancePlay(District x) {
        // 2023-05-30: last test game had problems with the Last district getting ahead of the others in time.
        if (x == Last) {
            var t0 = Last.LocalTime.TurnCounter;
            foreach(District d in m_DistrictsGrid) if (t0  > d.LocalTime.TurnCounter) return false;
        }
        return true;
    }

    private void initTurnOrder() {
      if (null != s_turn_order && s_turn_order.Length == m_Size*m_Size) return;
      List<Point> stage = new();
      stage.Add(Point.Empty);

      int scan = -1;
      while(stage.Count < m_Size*m_Size) {
        var tmp_E = stage[++scan] + Direction.E;
        var tmp_SW = stage[scan] + Direction.SW;
        if (InBounds(tmp_E) && !stage.Contains(tmp_E)) stage.Add(tmp_E);
        if (InBounds(tmp_SW) && !stage.Contains(tmp_SW)) stage.Add(tmp_SW);
      }

      s_turn_order = stage.ToArray();
#if DEBUG
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "World::initTurnOrder");
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "s_turn_order: " + s_turn_order.to_s());
#endif
    }

    private World()
    {
      var size = Engine.RogueGame.Options.CitySize;
#if DEBUG
      if (0 >= size) throw new ArgumentOutOfRangeException(nameof(size),size, "0 >= size");
#endif
      m_Size = (short)(size + 2);
      m_Extent = new Rectangle(Point.Empty, new Point(m_Size, m_Size));
      m_DistrictsGrid = new District[Size, Size];
//    Weather = Weather.CLEAR;
      var rules = Engine.Rules.Get;
      Weather = (Weather)(rules.Roll(0, (int)Weather._COUNT));
      NextWeatherCheckTurn = rules.Roll(WEATHER_MIN_DURATION, WEATHER_MAX_DURATION);  // alpha10

      m_CHAR_City = new Rectangle(CHAR_City_Origin,new Point(size, size));

      initTurnOrder();
    }

    [OnDeserialized] private void OnDeserialized(StreamingContext context)
    {
      m_Extent = new Rectangle(Point.Empty, new Point(m_Size, m_Size));
      var c_size = (short)(m_Size - 2);
      m_CHAR_City = new Rectangle(CHAR_City_Origin,new Point(c_size, c_size));
      s_Recent = this;
      initTurnOrder();
    }

    static public void Load(SerializationInfo info, StreamingContext context)
    {
      info.read_nullsafe(ref s_Recent, "World");
    }

    public void RepairLoad()
    {
      foreach(var d in m_DistrictsGrid) d.RepairLoad();
    }

    protected World(Zaimoni.Serialization.DecodeObjects decode)
    {
        byte relay_b = 0;
        int relay_i = 0;
        Zaimoni.Serialization.Formatter.Deserialize7bit(decode.src, ref m_Size);
        Zaimoni.Serialization.Formatter.Deserialize(decode.src, ref relay_b);
        Weather = (Weather)(relay_b);
        Zaimoni.Serialization.Formatter.Deserialize7bit(decode.src, ref relay_i);
        NextWeatherCheckTurn = relay_i;
        decode.LoadFrom(ref m_DistrictsGrid);

        ulong code;
        m_PlayerDistrict = decode.Load<District>(out code);
        if (0 < code && null == m_PlayerDistrict) {
            decode.Schedule(code, (o) => {
                if (o is District w) m_PlayerDistrict = w;
                else throw new InvalidOperationException("District object not loaded");
            });
        }

        m_SimDistrict = decode.Load<District>(out code);
        if (0 < code && null == m_SimDistrict) {
            decode.Schedule(code, (o) => {
                if (o is District w) m_SimDistrict = w;
                else throw new InvalidOperationException("District object not loaded");
            });
        }

        void onLoaded(District[] src) {
            foreach (var x in src) {
                m_Ready.Add(x);
            }
        }
        Zaimoni.Serialization.ISave.LinearLoad<District>(decode, onLoaded);

        m_CHAR_City = new Rectangle(CHAR_City_Origin,new Point(m_Size, m_Size));
        s_Recent = this;
        initTurnOrder();
    }

    private void SaveLoadOk(World test) {
        var err = string.Empty;

        if (m_Size != test.m_Size) err += "World size mismatch: "+m_Size.ToString()+ "" + test.m_Size.ToString();

        if (!string.IsNullOrEmpty(err)) throw new InvalidOperationException(err);
    }

    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode)
    {
        Zaimoni.Serialization.Formatter.Serialize7bit(encode.dest, m_Size);
        Zaimoni.Serialization.Formatter.Serialize(encode.dest, (byte)Weather);
        Zaimoni.Serialization.Formatter.Serialize7bit(encode.dest, NextWeatherCheckTurn);
        encode.SaveTo(m_DistrictsGrid);

        var code = encode.Saving(m_PlayerDistrict);
        if (0 < code) Zaimoni.Serialization.Formatter.SerializeObjCode(encode.dest, code);
        else Zaimoni.Serialization.Formatter.SerializeNull(encode.dest);
        code = encode.Saving(m_SimDistrict);
        if (0 < code) Zaimoni.Serialization.Formatter.SerializeObjCode(encode.dest, code);
        else Zaimoni.Serialization.Formatter.SerializeNull(encode.dest);
        Zaimoni.Serialization.ISave.LinearSave(encode, m_Ready);
    }

    static private int field_code(ref Utf8JsonReader reader) {
        if (reader.ValueTextEquals("Size")) return 1;
        else if (reader.ValueTextEquals("Weather")) return 2;
        else if (reader.ValueTextEquals("NextWeatherCheckTurn")) return 3;
        else if (reader.ValueTextEquals("Districts")) return 4;
        else if (reader.ValueTextEquals("PlayerDistrict")) return 5;
        else if (reader.ValueTextEquals("SimDistrict")) return 6;
        else if (reader.ValueTextEquals("Ready")) return 7;
        else if (reader.ValueTextEquals("Map_Links")) return 8;

        Engine.RogueGame.Game.ErrorPopup(reader.GetString());
        throw new JsonException();
    }

    private World(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
      if (JsonTokenType.StartObject != reader.TokenType) throw new JsonException();
      int origin_depth = reader.CurrentDepth;
      reader.Read();

      short relay_size = default;
      District[]? relay_districts = null;

      void read(ref Utf8JsonReader reader) {
          int code = field_code(ref reader);
          reader.Read();

          switch (code) {
          case 1:
              relay_size = reader.GetInt16();
              break;
          case 2:
              {
              string stage = reader.GetString();
              if (Enum.TryParse(stage, out Weather w)) {
                Weather = w;
                return;
              }
              Engine.RogueGame.Game.ErrorPopup("unrecognized Weather " + stage);
              }
              throw new JsonException();
          case 3:
              NextWeatherCheckTurn = reader.GetInt32();
              break;
          case 4:
              relay_districts = JsonSerializer.Deserialize<District[]>(ref reader, options) ?? throw new JsonException();
              break;
          case 5:
              m_PlayerDistrict = JsonSerializer.Deserialize<District>(ref reader, options) ?? throw new JsonException();
              break;
          case 6:
              m_SimDistrict = JsonSerializer.Deserialize<District>(ref reader, options) ?? throw new JsonException();
              break;
          case 7:
              {
              var stage = JsonSerializer.Deserialize<District[]>(ref reader, options) ?? throw new JsonException();
              foreach(var d in stage) m_Ready.Add(d);
              }
              break;
          case 8:
              {
              var stage = JsonSerializer.Deserialize<Location[]>(ref reader, options) ?? throw new JsonException();
              var scan = stage.Length;
              while (0 <= (scan -= 2)) {
                stage[scan].SetExit(stage[scan + 1]);
                stage[scan + 1].SetExit(stage[scan]);
              }
              }
              break;
          }
      }

      while (reader.CurrentDepth != origin_depth || JsonTokenType.EndObject != reader.TokenType) {
          if (JsonTokenType.PropertyName != reader.TokenType) throw new JsonException();

          read(ref reader);

          reader.Read();
      }

      if (JsonTokenType.EndObject != reader.TokenType) throw new JsonException();

      m_Size = relay_size;
      initTurnOrder();

      if (null == relay_districts) throw new ArgumentNullException(nameof(relay_districts));
      if (relay_districts.Length != m_Size * m_Size) throw new InvalidOperationException("tracing");
      m_DistrictsGrid = new District[m_Size,m_Size];
      foreach(var d in relay_districts) m_DistrictsGrid[d.WorldPosition.X, d.WorldPosition.Y] = d;
      relay_districts = null; // allow early garbage collection

#if PROTOTYPE
      // needs tile ids to save/load first
      DoForAllDistricts(District.InterpolateExits);
#endif

      // from OnDeserialized
      m_Extent = new Rectangle(Point.Empty, new Point(m_Size, m_Size));
      var c_size = (short)(m_Size - 2);
      m_CHAR_City = new Rectangle(CHAR_City_Origin,new Point(c_size, c_size));
//    s_Recent = this; // not until ready to take live
    }

    public static World fromJson(ref Utf8JsonReader reader, JsonSerializerOptions options) => new World(ref reader, options);

    public void toJson(Utf8JsonWriter writer, JsonSerializerOptions options) {
      writer.WriteStartObject();
      writer.WriteNumber("Size", m_Size);
      writer.WriteString("Weather", Weather.ToString());
      writer.WriteNumber("NextWeatherCheckTurn", NextWeatherCheckTurn);
      List<District>? stage = new();
      foreach(var d in m_DistrictsGrid) stage.Add(d);
      writer.WritePropertyName("Districts");
      JsonSerializer.Serialize(writer, stage.ToArray(), options);
      stage = null; // allow garbage collection
      if (null != m_PlayerDistrict) {
        writer.WritePropertyName("PlayerDistrict");
        JsonSerializer.Serialize(writer, m_PlayerDistrict, options);
      }
      if (null != m_SimDistrict) {
        writer.WritePropertyName("SimDistrict");
        JsonSerializer.Serialize(writer, m_SimDistrict, options);
      }
      writer.WritePropertyName("Ready");
      JsonSerializer.Serialize(writer, m_Ready.ToArray(), options);

      List<KeyValuePair<Location,Location>>? in_bounds_links = new();
      DoForAllMaps(m => {
          var stage = m.GetExits(pt => m.IsInBounds(pt));
          foreach (var x in stage) {
              KeyValuePair<Location, Location> alt = new(x.Value.Location, x.Key);
              if (in_bounds_links.Contains(alt)) continue;
              KeyValuePair<Location, Location> prime = new(x.Key, x.Value.Location);
              if (in_bounds_links.Contains(prime)) continue;
              in_bounds_links.Add(prime);
          }
      });
      if (0 < in_bounds_links.Count) {
        List<Location> relay_links = new();
        relay_links.Capacity = 2*in_bounds_links.Count;
        foreach(var x in in_bounds_links) {
          relay_links.Add(x.Key);
          relay_links.Add(x.Value);
        }
        in_bounds_links = null; // allow garbage collection
        writer.WritePropertyName("Map_Links");
        JsonSerializer.Serialize(writer, relay_links.ToArray(), options);
      }

      writer.WriteEndObject();
    }

    static public void Load(ref Utf8JsonReader reader)
    {
      var stage = JsonSerializer.Deserialize<World>(ref reader, Engine.Session.JSON_opts) ?? throw new JsonException();
//    s_Recent = stage; // when taking live
    }

    static public void Load(Zaimoni.Serialization.DecodeObjects decode)
    {
       // function extraction target does not work -- out/ref parameter needs accessing from lambda function
       var code = Zaimoni.Serialization.Formatter.DeserializeObjCode(decode.src);
       if (0 < code) {
           var obj = decode.Seen(code);
           if (null != obj) {
                    if (obj is World w) {
                        // s_Recent = w; // when taking live
                        s_Recent.SaveLoadOk(w); // when building out
                    }
                    else throw new InvalidOperationException("World object not loaded");
           } else {
                    decode.Schedule(code, (o) => {
                        if (o is World w) {
                            // s_Recent = w; // when taking live
                            s_Recent.SaveLoadOk(w); // when building out
                        }
                        else throw new InvalidOperationException("World object not loaded");
                    });
           }
        } else throw new InvalidOperationException("World object not loaded");
        // end failed function extraction target

//    s_Recent = stage; // when taking live
    }

    static public void Reset() => s_Recent = new World();

#if PROTOTYPE
    public Point toWorldPos(int n) { return new Point(n % m_Size, n / m_Size); }
    public int fromWorldPos(Point pt) { return pt.X + m_Size*pt.Y; }
    public int fromWorldPos(int x, int y) { return x + m_Size*y; }
#endif

    public HashSet<Actor>? PoliceInRadioRange(Location loc, Predicate<Actor>? test =null)
    {
      Location radio_pos = Engine.Rules.PoliceRadioLocation(loc);
      HashSet<Actor>? ret = null;
      DoForAllMaps(m=> {
          foreach (var a in m.Police.Get) {
              if (a.Location == loc) continue;
              if (null!=test && !test(a)) continue;
              Location other_radio_pos = Engine.Rules.PoliceRadioLocation(a.Location);
              if (Engine.RogueGame.POLICE_RADIO_RANGE >= Engine.Rules.GridDistance(radio_pos, in other_radio_pos)) (ret ??= new HashSet<Actor>()).Add(a); //  \todo change target for range reduction from being underground
          }
      });
      return ret;
    }

    public HashSet<Actor>? EveryoneInPoliceRadioRange(Location loc, Predicate<Actor>? test=null)
    {
      Location radio_pos = Engine.Rules.PoliceRadioLocation(loc);
      HashSet<Actor>? ret = null;
      DoForAllMaps(m=> {
          foreach (var a in m.Actors) {
              if (a.Location == loc) continue;
              if (!a.HasActivePoliceRadio) continue;
              if (null!=test && !test(a)) continue;
              Location other_radio_pos = Engine.Rules.PoliceRadioLocation(a.Location);
              if (Engine.RogueGame.POLICE_RADIO_RANGE >= Engine.Rules.GridDistance(radio_pos, in other_radio_pos)) (ret ??= new HashSet<Actor>()).Add(a); //  \todo change target for range reduction from being underground
          }
      });
      return ret;
    }

    public void DaimonMap()
    {
      if (!Engine.Session.Get.CMDoptionExists("socrates-daimon")) return;
      OutTextFile dest = new OutTextFile(Path.Combine(SetupConfig.DirPath, "daimon_map.html"));

      // initiate HTML page so we can style things properly
      dest.WriteLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">");
      dest.WriteLine("<html><head>");
      // CSS styles
      dest.WriteLine("<style type='text/css'>\n<!--");
      dest.WriteLine("pre {font-family: 'Courier New', monospace; font-size:15px}");
      dest.WriteLine(".inv {font-size:11px}");
      dest.WriteLine(".car {font-size:7px}");
      dest.WriteLine(".power {font-size:8px}");
      dest.WriteLine(".lfort {font-size:11px}");
      dest.WriteLine(".chair {font-size:17px}");
      dest.WriteLine("-->\n</style>");
      dest.WriteLine("</head><body>");

      District viewpoint = Engine.RogueGame.CurrentMap.District;

      var player = Engine.RogueGame.Player;
      var code = District.UsesCrossDistrictView(player.Location.Map);
      if (0 < code) {
        // simulate police radio view
        var zone = new ZoneLoc(player.Location.Map, new Rectangle(player.Location.Position - (Point)Engine.RogueGame.POLICE_RADIO_RANGE, (Point)(2* Engine.RogueGame.POLICE_RADIO_RANGE + 1)));
        (zone as IMap).DaimonMap(dest);
      }

      viewpoint.DaimonMap(dest);
      for(int x = 0; x<Size; x++) {
        for(int y = 0; y<Size; y++) {
          if (x== viewpoint.WorldPosition.X && y == viewpoint.WorldPosition.Y) continue;
          District d = this[x,y];
//        lock(d) { // this is causing a deadlock
            d.DaimonMap(dest);
//        }
        }
      }

      // typical HTML page termination
      dest.WriteLine("</body></html>");
      dest.Close();
    }

    public string WeatherChanges()
    {
      var rules = Engine.Rules.Get;
      NextWeatherCheckTurn = Engine.Session.Get.WorldTime.TurnCounter + rules.Roll(WEATHER_MIN_DURATION, WEATHER_MAX_DURATION);
      switch (Weather) {
        case Weather.CLEAR:
          Weather = Weather.CLOUDY;
          return "Clouds are hiding the sky.";
        case Weather.CLOUDY:
          if (rules.RollChance(50)) {
            Weather = Weather.CLEAR;
            return "The sky is clear again.";
          }
          Weather = Weather.RAIN;
          return "Rain is starting to fall.";
        case Weather.RAIN:
          if (rules.RollChance(50)) {
            Weather = Weather.CLOUDY;
            return "The rain has stopped.";
          }
          Weather = Weather.HEAVY_RAIN;
          return "The weather is getting worse!";
#if DEBUG
        case Weather.HEAVY_RAIN:
#else
        default:
#endif
          Weather = Weather.RAIN;
          return "The rain is less heavy.";
#if DEBUG
        default: throw new InvalidOperationException("unhandled weather");
#endif
      }
    }

    // possible micro-optimization target
    public int PlayerCount {
      get {
        int ret = 0;

        foreach(District d in m_DistrictsGrid) ret += d.PlayerCount;

        return ret;
      }
    }

    public List<District> PlayerDistricts {
      get {
        List<District> ret = new();

        foreach (District d in m_DistrictsGrid) {
          if (0 < d.PlayerCount) ret.Add(d);
        }

        return ret;
      }
    }

#region world-building support
    private void Rezone(Span<short> stats, DistrictKind[] zoning, DistrictKind dest, DistrictKind src) {
        var plan = new List<int>();
        int ub = zoning.Length;
        while(0 <= --ub) {
            if (src == zoning[ub]) plan.Add(ub);
        }
        int rezone = Engine.Rules.Get.DiceRoller.Choose(plan);
        stats[(int)src]--;
        stats[(int)dest]++;
        zoning[rezone] = dest;
    }

    private void SwapZones(DistrictKind[] zoning, Point view, DistrictKind dest, DistrictKind src) {
        var scan = new Rectangle(view + Direction.NW, (Point)3);
        var plan_dest = new List<int>();
        var plan_src = new List<int>();
        int ub = zoning.Length;
        while(0 <= --ub) {
            if (src == zoning[ub] && scan.Contains(CHAR_CityLimits.convert(ub))) plan_src.Add(ub);
            else if (dest == zoning[ub]) plan_dest.Add(ub);
            }
        var dr = Engine.Rules.Get.DiceRoller;
        int rezone_src = dr.Choose(plan_src);
        int rezone_dest = dr.Choose(plan_dest);
        zoning[rezone_src] = dest;
        zoning[rezone_dest] = src;
    }

    // General priority order for in-city districts
    private static readonly DistrictKind[] ZonePriority = { DistrictKind.SHOPPING, DistrictKind.GENERAL, DistrictKind.RESIDENTIAL, DistrictKind.BUSINESS, DistrictKind.GREEN };

    static private KeyValuePair<DistrictKind, DistrictKind> ExtremeZoning(Span<short> stats) {
        var max = new KeyValuePair<int, int>(0,int.MinValue);
        var min = new KeyValuePair<int, int>(0,int.MaxValue);
        int ub = stats.Length;
        while(0 <= --ub) {
            if (max.Value < stats[ub]) max = new KeyValuePair<int, int>(ub, stats[ub]);
            if (min.Value >= stats[ub]) min = new KeyValuePair<int, int>(ub, stats[ub]);
        }
        return new KeyValuePair<DistrictKind, DistrictKind>((DistrictKind)min.Key,(DistrictKind)max.Key);
    }

    private KeyValuePair<DistrictKind, DistrictKind> ExtremeZoning(Span<short> stats, DistrictKind[] zoning, Point view) {
        var scan = new Rectangle(view + Direction.NW, (Point)3);
        if (!CHAR_CityLimits.Contains(scan.Location) || !CHAR_CityLimits.Contains(scan.Location+scan.Size+Direction.NW))
#if DEBUG
            // invariant violation
            throw new InvalidOperationException("not-interior district sample: "+ CHAR_CityLimits.to_s()+" "+view.to_s()+" " + scan.to_s() + " " + scan.Location.to_s() + " " + CHAR_CityLimits.Contains(scan.Location).to_s() + " " + (scan.Location + scan.Size + Direction.NW).to_s() + " "+ CHAR_CityLimits.Contains(scan.Location + scan.Size + Direction.NW).to_s());
#else
            return default;
#endif
        stats.Fill(0);
        stats[(int)zoning[CHAR_CityLimits.convert(view)]]++;
        foreach(var dir in Direction.COMPASS) stats[(int)zoning[CHAR_CityLimits.convert(view + dir)]]++;

        var max = new KeyValuePair<int, int>(0,int.MinValue);
        var min = new KeyValuePair<int, int>(0,int.MaxValue);
        int ub = stats.Length;
        while(0 <= --ub) {
            if (max.Value < stats[ub]) max = new KeyValuePair<int, int>(ub, stats[ub]);
            if (min.Value >= stats[ub]) min = new KeyValuePair<int, int>(ub, stats[ub]);
        }
        return new KeyValuePair<DistrictKind, DistrictKind>((DistrictKind)min.Key,(DistrictKind)max.Key);
    }

    public DistrictKind[] PreliminaryZoning {
        get {
            const int in_city_district_ub = (int)DistrictKind.BUSINESS+1;

            Span<short> stats = stackalloc short[in_city_district_ub];
            Span<short> sample = stackalloc short[in_city_district_ub];
            var linear_extent = CitySize * CitySize;
            var ret = new DistrictKind[linear_extent];
            var rules = Engine.Rules.Get;
            foreach(ref var x in new Span<DistrictKind>(ret)) {
                var staging = rules.Roll(0, in_city_district_ub);
                stats[staging]++;
                x = (DistrictKind)staging;
            }

            var expected_all = linear_extent / in_city_district_ub;
            var unbalanced = ExtremeZoning(stats);
            var rare_priority = Array.IndexOf(ZonePriority, unbalanced.Key);
            var glut_priority = Array.IndexOf(ZonePriority, unbalanced.Value);

            while (expected_all > stats[(int)unbalanced.Key]) {
                int delta = stats[(int)unbalanced.Value] - stats[(int)unbalanced.Key];
                if (rare_priority > glut_priority && expected_all/2 >= delta) break;
                Rezone(stats, ret, unbalanced.Key, unbalanced.Value);
                unbalanced = ExtremeZoning(stats);
                rare_priority = Array.IndexOf(ZonePriority, unbalanced.Key);
                glut_priority = Array.IndexOf(ZonePriority, unbalanced.Value);
            }

            var city_center = CHAR_City_Origin + CHAR_CityLimits.Size / 2;
            const int expected_neighborhood = 9 / in_city_district_ub;
            const int require_neighborhood = (expected_neighborhood+1)/2;
            const int fluky_neighborhood = expected_neighborhood+1;

            var translation = new Rectangle(CHAR_City_Origin + Direction.SE, CHAR_CityLimits.Size + 2 * Direction.NW);
            Point pt_relay = default;
            int ub = translation.Size.X*translation.Size.Y;
            while(0 <= -- ub) {
                pt_relay = translation.convert(ub);
                unbalanced = ExtremeZoning(sample, ret, pt_relay);
                if (require_neighborhood <= sample[(int)unbalanced.Key] && fluky_neighborhood >= sample[(int)unbalanced.Value]) continue;
                SwapZones(ret, pt_relay, unbalanced.Key, unbalanced.Value);
                ub = translation.Size.X*translation.Size.Y;
            }

            // business district is plot-critical, but is covered by above
            return ret;
        }
    }
#endregion

    // Simulation support
    // the public functions all lock on m_PCready in order to ensure thread aborts don't leave us in
    // an inconsistent state
/*
 Idea here is to schedule the districts so that they never get "too far ahead" if we should want to build out cross-district pathfinding or line of sight.

 At game start for a 3x3 city, we have
 000
 000
 000

 i.e. only A1 is legal to run.  After it has run, we are at
 100
 000
 000

 We would like to schedule both A2 and B1, but A2 requires B1 to have already run.  After B1 has run:
 110
 000
 000

 B1 enables both C1 and A2 to be scheduled.  It's closer to "standard" to schedule C1 before A2.  After C1 and A2 runs:
 111
 100
 000

 we are now clear to schedule B2 (the first PC district in a 3x3 game).  After B2 has run
 111
 110
 000

 All of C2, A3, and A1 can be scheduled.  In "standard" we would defer A1 until after C2 had been scheduled, but that is a "global"
 constraint.  We do want A1 run twice before B2 is run twice.
 */
    private void ScheduleForAdvancePlay(District d)
    {
      if (d == m_PlayerDistrict) return;
      if (d == m_SimDistrict) return;
      if (m_Ready.Contains(d)) return;

      // these are based on morally readonly properties and thus can be used without a lock
      int t0 = Engine.Session.Get.WorldTime.TurnCounter;
#if DEBUG
      int t1 = At(Point.Empty)!.LocalTime.TurnCounter;
      if (t1 < t0) throw new InvalidOperationException("inverted district scheduling");
#endif

      int district_turn = d.LocalTime.TurnCounter;
#if DEBUG
      if (district_turn < t0) throw new InvalidOperationException("inverted district scheduling #2");
#endif

      if (district_turn > t0) {
#if DEBUG
        if (district_turn > Engine.Session.Get.WorldTime.TurnCounter+1) throw new InvalidOperationException("skew attempted");
        if (d.WorldPosition == Point.Empty) throw new InvalidOperationException("district scheduling sabotaged");
#endif
        return;
      }

      // we're clear.
      {
      var E_early = d.WorldPosition + Direction.E;
      var SW_early = d.WorldPosition + Direction.SW;
      var S_early = d.WorldPosition + Direction.SW;
      var SE_early = d.WorldPosition + Direction.SW;
      var scan = -1;
      while(m_Ready.Count > ++scan) {
        var pos = m_Ready[scan].WorldPosition;
        if (pos.Y == E_early.Y && pos.X >= E_early.X) {
          m_Ready.Insert(scan, d);
          return;
        }
        if (pos.Y == SW_early.Y && pos.X >= SW_early.X) {
          m_Ready.Insert(scan, d);
          return;
        }
      }

      }

      m_Ready.Add(d);
    }

    public void ScheduleAdjacentForAdvancePlay(District d)
    {
/*
550 run main : World::ScheduleAdjacentForAdvancePlay
551 run main : Interstate Highway@A0: 1
552 run main : Interstate Highway@A0: 1
553 run main : Interstate Highway@G6: 0
554 run main : Interstate Highway@B0: 0
555 run main : World::ScheduleAdjacentForAdvancePlay
556 run main : Interstate Highway@B0: 1
557 run main : Interstate Highway@A0: 1
558 run main : Interstate Highway@G6: 0
559 run main : Interstate Highway@C0: 0
560 run main : Interstate Highway@A1: 0
 */
      var t1 = At(Point.Empty)!.LocalTime.TurnCounter;
      var t0 = Last.LocalTime.TurnCounter;
      if (t1 == t0 || d == Last) {
        lock (m_Ready) {
#if DEBUG
          if (m_Ready.Contains(d)) throw new InvalidOperationException("already-complete district "+d.Name+" scheduled");
#endif
          Interlocked.CompareExchange(ref m_PlayerDistrict, null, d);
          Interlocked.CompareExchange(ref m_SimDistrict, null, d);
        }
        return;
      }
      if (t1 != t0+1) throw new InvalidOperationException("World::ScheduleAdjacentForAdvancePlay t1 "+ t1.ToString() + " t0 "+t0.ToString());

      var d_time = d.LocalTime.TurnCounter;
      if (t1 != d_time) throw new InvalidOperationException("World::ScheduleAdjacentForAdvancePlay t1 "+ t1.ToString() + " d_time " + d_time.ToString());

      // note that the turn counter goes up at the *start* of the next turn
#if AUDIT
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "World::ScheduleAdjacentForAdvancePlay");
      Logger.WriteLine(Logger.Stage.RUN_MAIN, d.Name + ": " + d.LocalTime.TurnCounter.ToString());
      Logger.WriteLine(Logger.Stage.RUN_MAIN, At(Point.Empty)!.Name + ": " + At(Point.Empty)!.LocalTime.TurnCounter.ToString());
      Logger.WriteLine(Logger.Stage.RUN_MAIN, Last.Name + ": " + Last.LocalTime.TurnCounter.ToString());
#endif

      // d.WorldPosition is morally readonly
      var tmp_E = At(d.WorldPosition + Direction.E);
      var tmp_SW = At(d.WorldPosition + Direction.SW);

      if (null != tmp_E) {
        var e_time = tmp_E.LocalTime.TurnCounter;

        if (t1 == e_time) tmp_E = null;
        else if (t0 != e_time) throw new InvalidOperationException("World::ScheduleAdjacentForAdvancePlay t0 "+ t0.ToString() + " e_time " + e_time.ToString());
      }

      if (null != tmp_SW) {
        var sw_time = tmp_SW.LocalTime.TurnCounter;
        if (t1 == sw_time) tmp_SW = null;
        else if (t0 != sw_time) throw new InvalidOperationException("World::ScheduleAdjacentForAdvancePlay t0 "+ t0.ToString() + " sw_time " + sw_time.ToString());
      }

      // other directions not needed.  An early protoype also used Direction.NW but this caused global vs. local time skew
#if AUDIT
      if (null != tmp_E) Logger.WriteLine(Logger.Stage.RUN_MAIN, tmp_E.Name + ": " + tmp_E.LocalTime.TurnCounter.ToString());
      if (null != tmp_SW) Logger.WriteLine(Logger.Stage.RUN_MAIN, tmp_SW.Name + ": " + tmp_SW.LocalTime.TurnCounter.ToString());
#endif

      lock (m_Ready) {
#if DEBUG
        if (m_Ready.Contains(d)) throw new InvalidOperationException("already-complete district "+d.Name+" scheduled");
#endif
        Interlocked.CompareExchange(ref m_PlayerDistrict, null, d);
        Interlocked.CompareExchange(ref m_SimDistrict, null, d);
        // the ones that would typically be scheduled
        if (null != tmp_E) ScheduleForAdvancePlay(tmp_E);
        if (null != tmp_SW) ScheduleForAdvancePlay(tmp_SW);
      }
    }

    static private Point[]? s_turn_order = null;

    private void bootstrap_districts() {
      if (null != m_PlayerDistrict || null != m_SimDistrict) return;
      lock (m_Ready) {
        if (0 < m_Ready.Count) return;

        int t0 = Engine.Session.Get.WorldTime.TurnCounter;
        List<Point> past = new();
        List<Point> now = new();
        List<Point> future = new();

        foreach(var d in m_DistrictsGrid) {
          if (d.LocalTime.TurnCounter == t0) now.Add(d.WorldPosition);
          else if (d.LocalTime.TurnCounter < t0) past.Add(d.WorldPosition);
          else future.Add(d.WorldPosition);
        }

#if AUDIT
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "World::bootstrap_districts");
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "past: " + past.Count.ToString()+" "+past.to_s());
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "now: " + now.Count.ToString()+" "+now.to_s());
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "future: " + future.Count.ToString()+" "+future.to_s());
#endif

        if (m_Size*m_Size == now.Count) {
          // Rogue Survivor 10- did not run events on the first turn of the game
          if (0 < Session.Get.WorldTime.TurnCounter) onBeforeTurn();
          foreach(var w_pos in s_turn_order!) m_Ready.Add(this[w_pos]);
          return;
        }
#if PROTOTYPE
        if (0 >= past.Count && now.Contains(Point.Empty)) {
          // Rogue Survivor 10- did not run events on the first turn of the game
          if (0 < Session.Get.WorldTime.TurnCounter) onBeforeTurn();
          m_Ready.Add(At(Point.Empty)!);
          return;
        }
        if (0 < past.Count && 0 >= future.Count) {
          m_Ready.Add(At(past[0])!);
          return;
        }
        if (0 >= past.Count && 0<now.Count) {
          m_Ready.Add(At(now[0])!);
          return;
        }
#endif
        throw new InvalidOperationException("unimplemented");
      }
    } 

    private void onBeforeTurn() {
      // C-style prefix bts_ for functions called from here.  UI is available due to call graph.
      Engine.RogueGame.Game.bts_TimeOfDayMessaging();
      Engine.RogueGame.Game.bts_DistrictEvents();
    }

    // avoiding property idiom for these as they affect World state
    public District? CurrentPlayerDistrict()
    {
      if (null != m_PlayerDistrict) return m_PlayerDistrict;
      if (null == m_SimDistrict) bootstrap_districts();
#if AUDIT
      var t1 = At(Point.Empty)!.LocalTime.TurnCounter;
      var t0 = Last.LocalTime.TurnCounter;
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "World::CurrentPlayerDistrict");
#endif
      lock (m_Ready) {
restart:
        if (0 >= m_Ready.Count) return null;
        District tmp = m_Ready[0];
        m_Ready.RemoveAt(0);
        var d_time = tmp.LocalTime.TurnCounter;
#if AUDIT
        Logger.WriteLine(Logger.Stage.RUN_MAIN, tmp.Name + ": " + tmp.LocalTime.TurnCounter.ToString());
        Logger.WriteLine(Logger.Stage.RUN_MAIN, At(Point.Empty)!.Name + ": " + At(Point.Empty)!.LocalTime.TurnCounter.ToString());
        Logger.WriteLine(Logger.Stage.RUN_MAIN, Last.Name + ": " + Last.LocalTime.TurnCounter.ToString());
#endif

        if (tmp.RequiresUI || null != m_SimDistrict) {
          Interlocked.CompareExchange(ref m_PlayerDistrict, tmp, null);
          return m_PlayerDistrict;
        }
        Interlocked.CompareExchange(ref m_SimDistrict, tmp, null);
        goto restart;
      }
    }

    public District? CurrentSimulationDistrict()
    {
      if (null != m_SimDistrict) return m_SimDistrict;
#if AUDIT
      var t1 = At(Point.Empty)!.LocalTime.TurnCounter;
      var t0 = Last.LocalTime.TurnCounter;
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "World::CurrentSimulationDistrict");
#endif
      lock (m_Ready) {
        if (0 >= m_Ready.Count) return null;
        int scan = -1;
        while(m_Ready.Count > ++scan) {
          if (m_Ready[scan].RequiresUI) continue;
#if AUDIT
          Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Ready[scan].Name + ": " + m_Ready[scan].LocalTime.TurnCounter.ToString());
          Logger.WriteLine(Logger.Stage.RUN_MAIN, At(Point.Empty)!.Name + ": " + t1.ToString());
          Logger.WriteLine(Logger.Stage.RUN_MAIN, Last.Name + ": " + t0.ToString());
#endif
          Interlocked.CompareExchange(ref m_SimDistrict, m_Ready[scan], null);
          m_Ready.RemoveAt(scan);
          return m_SimDistrict;
        }
      }
      return null;
    }

    private void _RejectActorActorInventoryCrossLink(List<string> errors, Actor origin)
    {
      var o_inv = origin.Inventory;
      if (null == o_inv || o_inv.IsEmpty) return;
      DoForAllActors(a => {
        if (a == origin) return;
        var err = a.Inventory?._HasCrossLink(o_inv);
        if (!string.IsNullOrEmpty(err)) errors.Add(origin.Name +", " + a.Name +" " + err);
      });
    }

    private void _RejectActorActorInventoryCrossLink(List<string> errors)
    {
      DoForAllActors(origin => _RejectActorActorInventoryCrossLink(errors, origin));
    }

    private void _RejectGroundGroundInventoryCrossLink(List<string> errors, Location o_loc, Inventory o_inv)
    {
      if (o_inv.IsEmpty) return;
      DoForAllGroundInventories((loc, inv) => {
        if (o_loc == loc) return;
        if (o_inv == inv) {
          errors.Add("bi-located inventory: "+o_loc+"; "+loc);
          return;
        }
        if (inv.IsEmpty) return;
        var err = inv._HasCrossLink(o_inv);
        if (!string.IsNullOrEmpty(err)) errors.Add(o_loc + "; " + loc + ": " + err);
      });
    }

    private void _RejectGroundGroundInventoryCrossLink(List<string> errors)
    {
      DoForAllGroundInventories((loc,inv) => _RejectGroundGroundInventoryCrossLink(errors, loc, inv));
    }

    private void _RejectActorGroundInventoryCrossLink(List<string> errors, Actor origin)
    {
      var o_inv = origin.Inventory;
      if (null == o_inv || o_inv.IsEmpty) return;
      DoForAllGroundInventories((loc, inv) => {
        if (inv.IsEmpty) return;
        var err = inv._HasCrossLink(o_inv);
        if (!string.IsNullOrEmpty(err)) errors.Add(origin.Name + "; " + loc + ": " + err);
      });
    }

    private void _RejectActorGroundInventoryCrossLink(List<string> errors)
    {
      DoForAllActors(origin => _RejectActorGroundInventoryCrossLink(errors, origin));
    }

    private void _RejectActorInventoryZero(List<string> errors)
    {
      DoForAllActors(a => {
        var err = a.Inventory?._HasZeroQuantityOrDuplicate();
        if (!string.IsNullOrEmpty(err)) errors.Add(a.Name +": " + err +": " + a.Inventory);
      });
    }

    private void _RejectActorInventoryMultiEquipped(List<string> errors)
    {
      DoForAllActors(a => {
        var err = a.Inventory?._HasMultiEquippedItems();
        if (!string.IsNullOrEmpty(err)) errors.Add(a.Name +": " + err +": " + a.Inventory);
      });
    }

    private void _RejectGroundInventoryZero(List<string> errors)    // multi-thread sensitive
    {
      DoForAllGroundInventories((loc,inv) => {
        if (loc.Map.District == (Engine.RogueGame.IsSimulating ? m_PlayerDistrict : m_SimDistrict)) return;
        var err = inv._HasZeroQuantityOrDuplicate();
        if (!string.IsNullOrEmpty(err)) errors.Add(loc + ": " + err +": " + inv);
      });
    }

    [Conditional("DEBUG")]
    public void _RejectInventoryDamage(List<string> errors)
    {
      // repairable checks
RestartActorZeroCheck:
      try {
        _RejectActorInventoryZero(errors);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartActorZeroCheck;
        throw;
      }
RestartActorMultiEquippedCheck:
      try {
        _RejectActorInventoryMultiEquipped(errors);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartActorMultiEquippedCheck;
        throw;
      }
RestartGroundZeroCheck:
      try {
        _RejectGroundInventoryZero(errors);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartGroundZeroCheck;
        throw;
      }
RestartActorGroundCrossLinkCheck:
      try {
        _RejectActorGroundInventoryCrossLink(errors);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartActorGroundCrossLinkCheck;
        throw;
      }
      // non-repairable checks (context-sensitive)
RestartActorActorCrossLinkCheck:
      try {
        _RejectActorActorInventoryCrossLink(errors);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartActorActorCrossLinkCheck;
        throw;
      }
RestartGroundGroundCrossLinkCheck:
      try {
        _RejectGroundGroundInventoryCrossLink(errors);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartGroundGroundCrossLinkCheck;
        throw;
      }
    }

    [Conditional("DEBUG")]
    public void _RejectInventoryDamage(List<string> errors, Actor a)
    {
RestartActorZeroCheck:
      try {
        _RejectActorInventoryZero(errors);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartActorZeroCheck;
        throw;
      }
RestartGroundZeroCheck:
      try {
        _RejectGroundInventoryZero(errors);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartGroundZeroCheck;
        throw;
      }
RestartActorGroundCrossLinkCheck:
      try {
        _RejectActorGroundInventoryCrossLink(errors, a);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartActorGroundCrossLinkCheck;
        throw;
      }
      // non-repairable checks (context-sensitive)
RestartActorActorCrossLinkCheck:
      try {
        _RejectActorActorInventoryCrossLink(errors, a);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartActorActorCrossLinkCheck;
        throw;
      }
    }

    public void TrimToBounds(ref Rectangle src)
    {
      var test = src.Left;
      if (0>test) {
        src.Width += test;
        src.X = 0;
      }

      test = src.Right;
      if (Size < test) src.Width -= (short)(test-Size);

      test = src.Top;
      if (0>test) {
        src.Height += test;
        src.Y = 0;
      }

      test = src.Bottom;
      if (Size < test) src.Width -= (short)(test-Size);
    }

    public static string CoordToString(int x, int y) => string.Format("{0}{1}", (char)(65 + x), y);
    public static string CoordToString(Point src) => string.Format("{0}{1}", (char)(65 + src.X), src.Y);
  }
}

namespace Zaimoni.JsonConvert
{
    public class World : System.Text.Json.Serialization.JsonConverter<djack.RogueSurvivor.Data.World>
    {
        public override djack.RogueSurvivor.Data.World Read(ref Utf8JsonReader reader, Type src, JsonSerializerOptions options)
        {
            return djack.RogueSurvivor.Data.World.fromJson(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, djack.RogueSurvivor.Data.World src, JsonSerializerOptions options)
        {
            src.toJson(writer, options);
        }
    }
}
