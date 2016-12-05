using Microsoft.Xna.Framework;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles;
using Winter_Defense.Managers;

namespace Winter_Defense.Particles
{
    class MapContainerModifier : IModifier
    {
        public float RestitutionCoefficient { get; set; } = 1;

        public unsafe void Update(float elapsedSeconds, ParticleBuffer.ParticleIterator iterator)
        {
            while (iterator.HasNext)
            {
                var particle = iterator.Next();

                var left = 0;
                var right = MapManager.Instance.MapWidth;
                var top = 0;
                var bottom = MapManager.Instance.MapHeight - MapManager.Instance.TileSize.Y;

                var xPos = particle->Position.X;
                var xVel = particle->Velocity.X;
                var yPos = particle->Position.Y;
                var yVel = particle->Velocity.Y;

                if ((int)particle->Position.X < left)
                {
                    xPos = left + (left - xPos);
                    xVel = -xVel * RestitutionCoefficient;
                }
                else if (particle->Position.X > right)
                {
                    xPos = right - (xPos - right);
                    xVel = -xVel * RestitutionCoefficient;
                }

                if (particle->Position.Y < top)
                {
                    yPos = top + (top - yPos);
                    yVel = -yVel * RestitutionCoefficient;
                }
                else if ((int)particle->Position.Y > bottom)
                {
                    yPos = bottom - (yPos - bottom);
                    yVel = -yVel * RestitutionCoefficient;
                    xVel = xVel * RestitutionCoefficient * 2;
                }
                particle->Position = new Vector2(xPos, yPos);
                particle->Velocity = new Vector2(xVel, yVel);
            }
        }
    }
}
