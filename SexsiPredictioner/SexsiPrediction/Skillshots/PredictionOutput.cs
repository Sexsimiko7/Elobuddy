﻿using System.Collections.Generic;
using EloBuddy;                   
using SharpDX;

namespace SexsiPrediction.Skillshots
{
    /// <summary>
    /// Class PredictionOutput.
    /// </summary>
    public class PredictionOutput
    {
        /// <summary>
        ///     Gets or sets the prediction input.
        /// </summary>
        /// <value>
        ///     The prediction input.
        /// </value>
        public PredictionInput Input { get; set; }

        /// <summary>
        ///     Gets or sets the collisions.
        /// </summary>
        /// <value>
        ///     The collisions.
        /// </value>
        public IList<GameObject> Collisions { get; set; } = new List<GameObject>();

        /// <summary>
        ///     Gets or sets the hit chance.
        /// </summary>
        /// <value>
        ///     The hit chance.
        /// </value>
        public HitChance HitChance { get; set; }

        /// <summary>
        ///     Gets or sets the cast position.
        /// </summary>
        /// <value>
        ///     The cast position.
        /// </value>
        public Vector3 CastPosition { get; set; }

        /// <summary>
        ///     Gets or sets the target's predicted position.
        /// </summary>
        /// <value>
        ///     The target's predicted position.
        /// </value>
        public Vector3 PredictedPosition { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the <see cref="CastPosition" /> will collide with another GameObject.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the <see cref="CastPosition" /> will collide with another GameObject; otherwise, <c>false</c>.
        /// </value>
        public bool Collision => this.Collisions.Count > 0 || this.HitChance == HitChance.Collision;

        /// <summary>
        /// Gets or sets the number of targets hit by the area of effect spell.
        /// </summary>
        /// <value>The number of targets hit by the area of effect spell.</value>
        public int AoeTargetsHit { get; set; }
    }
}
