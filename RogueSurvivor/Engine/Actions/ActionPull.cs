using System;
using djack.RogueSurvivor.Data;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Engine.Actions
{
    internal interface ObjectOrigin
    {
        Location obj_origin { get; }  // of m_Actor
    }

    [Serializable]
    class ActionPull : ActorAction, ActorDest, ObjectOrigin, IJsonStaged<ActionPull>
    {
        #region Fields
        readonly MapObject m_Object;
        readonly Location m_MoveActorTo;
        #endregion

        public Location dest { get { return m_MoveActorTo; } }
        public Location obj_origin { get { return m_Object.Location; } }

        #region Init
        #region JSON save/load
        private ActionPull(Actor actor, JsonElement src, JsonSerializerOptions options) : base(actor)
        {
            m_Object = src.Require("Object").RequireRef<MapObject>();
            m_MoveActorTo = src.Require("MoveTo").Deserialize<Location>(options);
        }

        static ActionPull IJsonStaged<ActionPull>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new ActionPull(JsonActor(src), src, options));

        public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            toJsonActor(writer);
            writer.WriteRef("Object", m_Object);
            writer.WritePropertyName("MoveTo");
            JsonSerializer.Serialize(writer, m_MoveActorTo, options);
        }
        #endregion

        public ActionPull(Actor actor, MapObject pullObj, Direction moveActorDir) : base(actor)
        {
            m_Object = pullObj
#if DEBUG
                ?? throw new ArgumentNullException(nameof(pullObj))
#endif
            ;
            m_MoveActorTo = m_Actor.Location + moveActorDir;
            if (!Map.Canonical(ref m_MoveActorTo)) throw new ArgumentNullException(nameof(m_MoveActorTo));
        }

        public ActionPull(Actor actor, MapObject pullObj, Location moveActorTo) : base(actor)
        {
            if (!Map.Canonical(ref moveActorTo)) throw new ArgumentNullException(nameof(moveActorTo));
            m_Object = pullObj
#if DEBUG
                ?? throw new ArgumentNullException(nameof(pullObj))
#endif
            ;
            m_MoveActorTo = moveActorTo;
        }
        #endregion

        #region ActorAction
        public override bool IsLegal()
        {
            return m_Actor.CanPull(m_Object, in m_MoveActorTo, out m_FailReason);
        }

        public override bool IsPerformable()
        {
            if (!base.IsPerformable()) return false;
            return  1==Rules.GridDistance(m_Actor.Location, m_Object.Location)   // no pull/push through vertical exits
                 && 1==Rules.GridDistance(m_Actor.Location, m_MoveActorTo);
        }

        public override void Perform()
        {
            RogueGame.Game.DoPull(m_Actor, m_Object, in m_MoveActorTo);
        }
        #endregion

    }
}
