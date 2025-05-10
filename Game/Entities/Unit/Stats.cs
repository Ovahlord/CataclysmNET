using Game.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Entities.Unit
{
    public sealed class Stats
    {
        private readonly float[] _baseMovementSpeed = new float[(byte)MovementType.Max]
        {
            2.5f,      // MOVE_WALK
            7.0f,      // MOVE_RUN
            4.5f,      // MOVE_RUN_BACK
            4.722222f, // MOVE_SWIM
            2.5f,      // MOVE_SWIM_BACK
            3.141594f, // MOVE_TURN_RATE
            7.0f,      // MOVE_FLIGHT
            4.5f,      // MOVE_FLIGHT_BACK
            3.14f      // MOVE_PITCH_RATE
        };

        private readonly float[] _movementSpeedMultiplier = new float[(byte)MovementType.Max]
        {
            1f, // MOVE_WALK
            1f, // MOVE_RUN
            1f, // MOVE_RUN_BACK
            1f, // MOVE_SWIM
            1f, // MOVE_SWIM_BACK
            1f, // MOVE_TURN_RATE
            1f, // MOVE_FLIGHT
            1f, // MOVE_FLIGHT_BACK
            1f  // MOVE_PITCH_RATE
        };

        public float WalkSpeed          { get { return _baseMovementSpeed[(byte)MovementType.Walk] * _movementSpeedMultiplier[(byte)MovementType.Walk]; } }
        public float RunSpeed           { get { return _baseMovementSpeed[(byte)MovementType.Run] * _movementSpeedMultiplier[(byte)MovementType.Run]; } }
        public float RunBackSpeed       { get { return _baseMovementSpeed[(byte)MovementType.RunBack] * _movementSpeedMultiplier[(byte)MovementType.RunBack]; } }
        public float SwimSpeed          { get { return _baseMovementSpeed[(byte)MovementType.Swim] * _movementSpeedMultiplier[(byte)MovementType.Swim]; } }
        public float SwimBackSpeed      { get { return _baseMovementSpeed[(byte)MovementType.SwimBack] * _movementSpeedMultiplier[(byte)MovementType.SwimBack]; } }
        public float TurnRate           { get { return _baseMovementSpeed[(byte)MovementType.TurnRate] * _movementSpeedMultiplier[(byte)MovementType.TurnRate]; } }
        public float FlightSpeed        { get { return _baseMovementSpeed[(byte)MovementType.Flight] * _movementSpeedMultiplier[(byte)MovementType.Flight]; } }
        public float FlightBackSpeed    { get { return _baseMovementSpeed[(byte)MovementType.FlightBack] * _movementSpeedMultiplier[(byte)MovementType.FlightBack]; } }
        public float PitchRate          { get { return _baseMovementSpeed[(byte)MovementType.PitchRate] * _movementSpeedMultiplier[(byte)MovementType.PitchRate]; } }

    }
}
