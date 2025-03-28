//
// This autonomous intelligent system software is the property of Cartheur Research, Copyright 2018 - 2025, all rights reserved.
//
using System.Collections.Generic;

namespace NaoRemote.Proxy
{

    public class ProxyMotion
    {

        private const string ModuleName = "ALMotion";
        private readonly NaoRemoteModule _naoModule;
        private readonly bool _isPost;

        public ProxyMotion(NaoRemoteModule naoModule, bool isPost)
        {
            _naoModule = naoModule;
            _isPost = isPost;
        }

        #region Cartesian control API

        public void PositionInterpolation(string chainName, int space, List<double> position, int axisMask, double duration, bool isAbsolute)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "positionInterpolation", chainName, space, position, axisMask, duration, isAbsolute);
        }

        public void PositionInterpolation(string chainName, int space, List<List<double>> path, int axisMask, List<double> durations, bool isAbsolute)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "positionInterpolation", chainName, space, path, axisMask, durations, isAbsolute);
        }

        public void PositionInterpolations(List<string> effectorNames, int taskSpaceForAllPaths, List<List<List<double>>> paths, List<int> axisMasks, List<List<double>> relativeTimes, bool isAbsolute)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "positionInterpolations", effectorNames, taskSpaceForAllPaths, paths, axisMasks, relativeTimes, isAbsolute);
        }

        public void SetPosition(string chainName, int space, List<double> position, double fractionMaxSpeed, int axisMask)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "setPosition", chainName, space, position, fractionMaxSpeed, axisMask);
        }

        public void ChangePosition(string effectorName, int space, List<double> positionChange, double fractionMaxSpeed, int axisMask)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "changePosition", effectorName, space, positionChange, fractionMaxSpeed, axisMask);
        }

        public List<double> GetPosition(string name, int space, bool useSensorValues)
        {
            return _naoModule.Eval<List<double>>(ModuleName, _isPost, "getPosition", name, space, useSensorValues);
        }

        public void TransformInterpolation(string chainName, int space, List<double> transform, int axisMask, double duration, bool isAbsolute)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "transformInterpolation", chainName, space, transform, axisMask, duration, isAbsolute);
        }

        public void TransformInterpolation(string chainName, int space, List<List<double>> path, int axisMask, List<double> durations, bool isAbsolute)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "transformInterpolation", chainName, space, path, axisMask, durations, isAbsolute);
        }

        public void TransformInterpolations(List<string> effectorNames, int taskSpaceForAllPaths, List<List<List<double>>> paths, List<int> axisMasks, List<List<double>> durations, bool isAbsolute)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "transformInterpolations", effectorNames, taskSpaceForAllPaths, paths, axisMasks, durations, isAbsolute);
        }

        public void SetTransform(string chainName, int space, List<double> transform, double fractionMaxSpeed, int axisMask )
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "setTransform", space, transform, fractionMaxSpeed, axisMask);
        }

        public void ChangeTransform(string chainName, int space, List<double> transform, double fractionMaxSpeed, int axisMask)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "changeTransform", space, transform, fractionMaxSpeed, axisMask);
        }

        public List<double> GetTransform(string name, int space, bool useSensorValues)
        {
            return _naoModule.Eval<List<double>>(ModuleName, _isPost, "getTransform", name, space, useSensorValues);
        }

        #endregion

        #region Joint control API

        public void AngleInterpolation(string name, double angle, double time, bool isAbsolute)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "angleInterpolation", name, angle, time, isAbsolute);
        }

        public void AngleInterpolation(string name, List<double> angles, List<double> times, bool isAbsolute)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "angleInterpolation", name, angles, times, isAbsolute);
        }

        public void AngleInterpolation(List<string> names, List<List<double>> angles, List<List<double>> times, bool isAbsolute)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "angleInterpolation", names, angles, times, isAbsolute);
        }

        public void AngleInterpolationWithSpeed(string name, double targetAngle, double maxSpeedFraction)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "angleInterpolationWithSpeed", name, targetAngle, maxSpeedFraction);
        }

        public void AngleInterpolationWithSpeed(List<string> names, List<double> targetAngles, double maxSpeedFraction)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "angleInterpolationWithSpeed", names, targetAngles, maxSpeedFraction);
        }

        public void AngleInterpolationWithSpeed(string name, List<double> targetAngles, double maxSpeedFraction)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "angleInterpolationWithSpeed", name, targetAngles, maxSpeedFraction);
        }

        public void AngleInterpolationBezier(List<string> jointNames, List<List<double>> times, List<List<List<object>>> controlPoints)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "angleInterpolationBezier", jointNames, times, controlPoints);
        }

        public void SetAngles(string name, double angle, double fractionMaxSpeed)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "setAngles", name, angle, fractionMaxSpeed);
        }

        public void SetAngles(List<string> names, List<double> angles, double fractionMaxSpeed)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "setAngles", names, angles, fractionMaxSpeed);
        }

        public void SetAngles(string name, List<double> angles, double fractionMaxSpeed)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "setAngles", name, angles, fractionMaxSpeed);
        }

        public void ChangeAngles(string name, double change, double fractionMaxSpeed)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "changeAngles", name, change, fractionMaxSpeed);
        }

        public void ChangeAngles(List<string> names, List<double> changes, double fractionMaxSpeed)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "changeAngles", names, changes, fractionMaxSpeed);
        }

        public void ChangeAngles(string name, List<double> changes, double fractionMaxSpeed)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "changeAngles", name, changes, fractionMaxSpeed);
        }

        public List<double> GetAngles(string name, bool useSensors)
        {
            return _naoModule.Eval<List<double>>(ModuleName, _isPost, "getAngles", name, useSensors);
        }

        public List<double> GetAngles(List<string> names, bool useSensors)
        {
            return _naoModule.Eval<List<double>>(ModuleName, _isPost, "getAngles", names, useSensors);
        }

        public void CloseHand(string handName)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "closeHand", handName);
        }

        public void OpenHand(string handName)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "openHand", handName);
        }

        #endregion

        #region Stiffness control API

        public void WakeUp()
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "wakeUp");
        }

        public void Rest()
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "rest");
        }

        public void StiffnessInterpolation(string name, double stiffness, double time)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "stiffnessInterpolation", name, stiffness, time);
        }

        public void StiffnessInterpolation(string name, List<double> listStiffness, List<double> times)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "stiffnessInterpolation", name, listStiffness, times);
        }

        public void StiffnessInterpolation(List<string> names, List<double> listStiffness, List<double> times)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "stiffnessInterpolation", names, listStiffness, times);
        }

        public void StiffnessInterpolation(List<string> names, List<List<double>> listStiffness, List<List<double>> times)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "stiffnessInterpolation", names, listStiffness, times);
        }

        public void SetStiffnesses(string name, double stiffness)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "setStiffnesses", name, stiffness);
        }

        public void SetStiffnesses(List<string> names, List<double> listStiffness)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "setStiffnesses", names, listStiffness);
        }

        public double GetStiffnesses(string name)
        {
            return _naoModule.Eval<double>(ModuleName, _isPost, "getStiffnesses", name);
        }

        public List<double> GetStiffnesses(List<string> names)
        {
            return _naoModule.Eval<List<double>>(ModuleName, _isPost, "getStiffnesses", names);
        }

        #endregion
    }
}
