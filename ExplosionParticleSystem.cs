using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace RetroHeroes
{
    public class ExplosionParticleSystem : ParticleSystem
    {
        public ExplosionParticleSystem(Game game, int maxExplosions) : base(game, maxExplosions * 25)
        {

        }

        protected override void InitializeConstants()
        {
            textureFilename = "explosion";

            minNumParticles = 4;
            maxNumParticles = 6;

            blendState = BlendState.Additive;
            DrawOrder = AdditiveBlendDrawOrder;
        }

        protected override void InitializeParticle(ref Particle p, Vector2 where)
        {
            var velocity = RandomHelper.NextDirection() * RandomHelper.NextFloat(0.1f, 0.5f);

            var lifetime = RandomHelper.NextFloat(0.25f, 0.5f);

            var rotation = RandomHelper.NextFloat(0, MathHelper.TwoPi);

            var angularVelocity = RandomHelper.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);

            var acceleration = -velocity / lifetime;

            var scale = RandomHelper.NextFloat(0.01f, 0.02f);

            p.Initialize(where, velocity, acceleration, lifetime: lifetime, rotation: rotation, angularVelocity: angularVelocity, scale: scale);            
        }

        protected override void UpdateParticle(ref Particle particle, float dt)
        {
            base.UpdateParticle(ref particle, dt);

            float nomalizeLifetime = particle.TimeSinceStart / particle.Lifetime;

            float alphaValue = 4 * nomalizeLifetime * (1 - nomalizeLifetime);

            particle.Color = Color.White * alphaValue;

            particle.Scale = 0.4f + 0.25f * nomalizeLifetime;
        }

        public void PlaceExplosion(Vector2 where)
        {
            AddParticles(where);
        }
    }
}
