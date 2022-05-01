using System;
using System.Diagnostics;
using System.Reflection;

namespace NullSoftware.Serialization
{
    /// <summary>
    /// Wrapper for <see cref="System.Reflection.MemberInfo"/> 
    /// to provide easy access to get or set methods. 
    /// </summary>
    /// <remarks>
    /// This class is created to work with <see cref="FieldInfo"/> or <see cref="PropertyInfo"/>.
    /// </remarks>
    internal sealed class MemberInfoProxy : MemberInfo
    {
        private Func<object, object> _getMethod;
        private Action<object, object> _setMethod;

        /// <summary>
        /// Gets wrapped member.
        /// </summary>
        public MemberInfo MemberInfo { get; }

        /// <summary>
        /// Gets original type of field or property.
        /// </summary>
        public Type MemberTargetType { get; }

        /// <summary>
        /// Gets safe type of field or property, that can be used by serializer.
        /// </summary>
        public Type SafeMemberTargetType { get; }

        /// <summary>
        /// Gets a value indicating whether the current member is readonly.
        /// </summary>
        public bool IsReadOnly { get; }

        #region Override Properties

        /// <inheritdoc/>
        public override string Name => MemberInfo.Name;

        /// <inheritdoc/>
        public override Type DeclaringType => MemberInfo.DeclaringType;

        /// <inheritdoc/>
        public override MemberTypes MemberType => MemberInfo.MemberType;

        /// <inheritdoc/>
        public override Type ReflectedType => MemberInfo.ReflectedType;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberInfoProxy"/> 
        /// based on <see cref="FieldInfo"/>.
        /// </summary>
        /// <param name="field">Field Info.</param>
        public MemberInfoProxy(FieldInfo field)
        {
            MemberInfo = field;
            MemberTargetType = field.FieldType;
            SafeMemberTargetType = GetSafeType(field.FieldType);
            IsReadOnly = field.IsInitOnly;
            _getMethod = field.GetValue;
            _setMethod = field.SetValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberInfoProxy"/>
        /// based on <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="prop">Property Info.</param>
        public MemberInfoProxy(PropertyInfo prop)
        {
            MemberInfo = prop;
            MemberTargetType = prop.PropertyType;
            SafeMemberTargetType = GetSafeType(prop.PropertyType);
            IsReadOnly = !prop.CanWrite;
            _getMethod = prop.GetValue;
            _setMethod = prop.SetValue;
        }

        /// <summary>
        /// Returns a field of property value of a specified object.
        /// </summary>
        /// <param name="obj">The object whose field/property value will be returned.</param>
        /// <returns>The field of property value of a specified object.</returns>
        [DebuggerStepThrough]
        public object GetValue(object obj)
        {
            return _getMethod(obj);
        }

        /// <summary>
        /// Sets the field or property value of a specified object.
        /// </summary>
        /// <param name="obj">The object whose field/property value will be set.</param>
        /// <param name="value">The new field or property value.</param>
        [DebuggerStepThrough]
        public void SetValue(object obj, object value)
        {
            _setMethod(obj, value);
        }

        /// <summary>
        /// Gets safe type for serializer. 
        /// For enum type this method returns its underlying type.
        /// </summary>
        /// <param name="memberType">Type of member.</param>
        /// <returns>The serializer safe type.</returns>
        private Type GetSafeType(Type memberType)
        {
            if (memberType.IsEnum)
                return memberType.GetEnumUnderlyingType();
            else
                return memberType;
        }

        #region Override Methods

        /// <inheritdoc/>
        [DebuggerStepThrough]
        public override object[] GetCustomAttributes(bool inherit)
        {
            return MemberInfo.GetCustomAttributes(inherit);
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return MemberInfo.GetCustomAttributes(attributeType, inherit);
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return MemberInfo.IsDefined(attributeType, inherit);
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        public override string ToString()
        {
            return MemberInfo.ToString();
        }

        #endregion

        #region Operators

        public static explicit operator FieldInfo(MemberInfoProxy proxy) => (FieldInfo)proxy.MemberInfo;
        public static explicit operator PropertyInfo(MemberInfoProxy proxy) => (PropertyInfo)proxy.MemberInfo;

        #endregion
    }
}