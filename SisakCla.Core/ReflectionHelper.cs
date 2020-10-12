using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SisakCla.Core
{
    internal static class ReflectionHelper
    {
        public static IEnumerable<MethodInfo> GetMethodsWithAttribute(Type classType, Type attributeType)
        {
            return classType.GetMethods().Where(methodInfo => methodInfo.GetCustomAttributes(attributeType, true).Length > 0);
        }

        public static IEnumerable<MemberInfo> GetMembersWithAttribute(Type classType, Type attributeType)
        {
            return classType.GetMembers().Where(memberInfo => memberInfo.GetCustomAttributes(attributeType, true).Length > 0);
        }

        public static IEnumerable<PropertyInfo> GetPropertiesWithAttribute(Type classType, Type attributeType) 
        {
            return classType.GetProperties().Where(propInfo => propInfo.GetCustomAttributes(attributeType, true).Length > 0);
        }

        public static IEnumerable<FieldInfo> GetFieldsWithAttribute(Type classType, Type attributeType)
        {
            return classType.GetFields().Where(fieldInfo => fieldInfo.GetCustomAttributes(attributeType, true).Length > 0);
        }
    }
}