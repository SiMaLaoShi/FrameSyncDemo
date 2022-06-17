﻿#if !NO_RUNTIME

#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
using IKVM.Reflection;
#else
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ProtoBuf.Meta;
#endif

namespace ProtoBuf.Serializers
{
    internal sealed class ImmutableCollectionDecorator : ListDecorator
    {
        protected override bool RequireAdd
        {
            get { return false; }
        }
#if !NO_GENERICS

        private static Type ResolveIReadOnlyCollection(Type declaredType, Type t)
        {
#if WINRT || COREFX
            foreach (Type intImplBasic in declaredType.GetTypeInfo().ImplementedInterfaces)
            {
                TypeInfo intImpl = intImplBasic.GetTypeInfo();
                if (intImpl.IsGenericType && intImpl.Name.StartsWith("IReadOnlyCollection`"))
                {
                    if(t != null)
                    {
                        Type[] typeArgs = intImpl.GenericTypeArguments;
                        if (typeArgs.Length != 1 && typeArgs[0] != t) continue;
                    }
                    return intImplBasic;
                }
            }
#else
            foreach (var intImpl in declaredType.GetInterfaces())
                if (intImpl.IsGenericType && intImpl.Name.StartsWith("IReadOnlyCollection`"))
                {
                    if (t != null)
                    {
                        var typeArgs = intImpl.GetGenericArguments();
                        if (typeArgs.Length != 1 && typeArgs[0] != t) continue;
                    }

                    return intImpl;
                }
#endif
            return null;
        }

        internal static bool IdentifyImmutable(TypeModel model, Type declaredType, out MethodInfo builderFactory,
            out MethodInfo add, out MethodInfo addRange, out MethodInfo finish)
        {
            builderFactory = add = addRange = finish = null;
            if (model == null || declaredType == null) return false;
#if WINRT || COREFX
            TypeInfo declaredTypeInfo = declaredType.GetTypeInfo();
#else
            var declaredTypeInfo = declaredType;
#endif

            // try to detect immutable collections; firstly, they are all generic, and all implement IReadOnlyCollection<T> for some T
            if (!declaredTypeInfo.IsGenericType) return false;

#if WINRT || COREFX
            Type[] typeArgs = declaredTypeInfo.GenericTypeArguments, effectiveType;
#else
            Type[] typeArgs = declaredTypeInfo.GetGenericArguments(), effectiveType;
#endif
            switch (typeArgs.Length)
            {
                case 1:
                    effectiveType = typeArgs;
                    break; // fine
                case 2:
                    var kvp = model.MapType(typeof(KeyValuePair<,>));
                    if (kvp == null) return false;
                    kvp = kvp.MakeGenericType(typeArgs);
                    effectiveType = new[] { kvp };
                    break;
                default:
                    return false; // no clue!
            }

            if (ResolveIReadOnlyCollection(declaredType, null) == null) return false; // no IReadOnlyCollection<T> found

            // and we want to use the builder API, so for generic Foo<T> or IFoo<T> we want to use Foo.CreateBuilder<T>
            var name = declaredType.Name;
            var i = name.IndexOf('`');
            if (i <= 0) return false;
            name = declaredTypeInfo.IsInterface ? name.Substring(1, i - 1) : name.Substring(0, i);

            var outerType = model.GetType(declaredType.Namespace + "." + name, declaredTypeInfo.Assembly);
            // I hate special-cases...
            if (outerType == null && name == "ImmutableSet")
                outerType = model.GetType(declaredType.Namespace + ".ImmutableHashSet", declaredTypeInfo.Assembly);
            if (outerType == null) return false;

#if WINRT
            foreach (MethodInfo method in outerType.GetTypeInfo().DeclaredMethods)
#else
            foreach (var method in outerType.GetMethods())
#endif
            {
                if (!method.IsStatic || method.Name != "CreateBuilder" || !method.IsGenericMethodDefinition ||
                    method.GetParameters().Length != 0
                    || method.GetGenericArguments().Length != typeArgs.Length) continue;

                builderFactory = method.MakeGenericMethod(typeArgs);
                break;
            }

            var voidType = model.MapType(typeof(void));
            if (builderFactory == null || builderFactory.ReturnType == null ||
                builderFactory.ReturnType == voidType) return false;


            add = Helpers.GetInstanceMethod(builderFactory.ReturnType, "Add", effectiveType);
            if (add == null) return false;

            finish = Helpers.GetInstanceMethod(builderFactory.ReturnType, "ToImmutable", Helpers.EmptyTypes);
            if (finish == null || finish.ReturnType == null || finish.ReturnType == voidType) return false;

            if (!(finish.ReturnType == declaredType || Helpers.IsAssignableFrom(declaredType, finish.ReturnType)))
                return false;

            addRange = Helpers.GetInstanceMethod(builderFactory.ReturnType, "AddRange", new[] { declaredType });
            if (addRange == null)
            {
                var enumerable = model.MapType(typeof(IEnumerable<>), false);
                if (enumerable != null)
                    addRange = Helpers.GetInstanceMethod(builderFactory.ReturnType, "AddRange",
                        new[] { enumerable.MakeGenericType(effectiveType) });
            }

            return true;
        }
#endif
        private readonly MethodInfo builderFactory, add, addRange, finish;
        internal ImmutableCollectionDecorator(TypeModel model, Type declaredType, Type concreteType,
            IProtoSerializer tail, int fieldNumber, bool writePacked, WireType packedWireType, bool returnList,
            bool overwriteList, bool supportNull,
            MethodInfo builderFactory, MethodInfo add, MethodInfo addRange, MethodInfo finish)
            : base(model, declaredType, concreteType, tail, fieldNumber, writePacked, packedWireType, returnList,
                overwriteList, supportNull)
        {
            this.builderFactory = builderFactory;
            this.add = add;
            this.addRange = addRange;
            this.finish = finish;
        }
#if !FEAT_IKVM
        public override object Read(object value, ProtoReader source)
        {
            var builderInstance = builderFactory.Invoke(null, null);
            var field = source.FieldNumber;
            var args = new object[1];
            if (AppendToCollection && value != null && ((IList)value).Count != 0)
            {
                if (addRange != null)
                {
                    args[0] = value;
                    addRange.Invoke(builderInstance, args);
                }
                else
                {
                    foreach (var item in (IList)value)
                    {
                        args[0] = item;
                        add.Invoke(builderInstance, args);
                    }
                }
            }

            if (packedWireType != WireType.None && source.WireType == WireType.String)
            {
                var token = ProtoReader.StartSubItem(source);
                while (ProtoReader.HasSubValue(packedWireType, source))
                {
                    args[0] = Tail.Read(null, source);
                    add.Invoke(builderInstance, args);
                }

                ProtoReader.EndSubItem(token, source);
            }
            else
            {
                do
                {
                    args[0] = Tail.Read(null, source);
                    add.Invoke(builderInstance, args);
                } while (source.TryReadFieldHeader(field));
            }

            return finish.Invoke(builderInstance, null);
        }
#endif

#if FEAT_COMPILER
        protected override void EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            using (Compiler.Local oldList = AppendToCollection ? ctx.GetLocalWithValue(ExpectedType, valueFrom) : null)
            using(Compiler.Local builder = new Compiler.Local(ctx, builderFactory.ReturnType))
            {
                ctx.EmitCall(builderFactory);
                ctx.StoreValue(builder);

                if(AppendToCollection)
                {
                    Compiler.CodeLabel done = ctx.DefineLabel();
                    if(!Helpers.IsValueType(ExpectedType))
                    {
                        ctx.LoadValue(oldList);
                        ctx.BranchIfFalse(done, false); // old value null; nothing to add
                    }
#if COREFX
                    TypeInfo typeInfo = ExpectedType.GetTypeInfo();
#else
                    Type typeInfo = ExpectedType;
#endif
                    PropertyInfo prop = Helpers.GetProperty(typeInfo, "Length", false);
                    if(prop == null) prop = Helpers.GetProperty(typeInfo, "Count", false);
#if !NO_GENERICS
                    if (prop == null) prop =
 Helpers.GetProperty(ResolveIReadOnlyCollection(ExpectedType, Tail.ExpectedType), "Count", false);
#endif
                    ctx.LoadAddress(oldList, oldList.Type);
                    ctx.EmitCall(Helpers.GetGetMethod(prop, false, false));
                    ctx.BranchIfFalse(done, false); // old list is empty; nothing to add

                    Type voidType = ctx.MapType(typeof(void));
                    if(addRange != null)
                    {
                        ctx.LoadValue(builder);
                        ctx.LoadValue(oldList);
                        ctx.EmitCall(addRange);
                        if (addRange.ReturnType != null && add.ReturnType != voidType) ctx.DiscardValue();
                    }
                    else
                    {
                        // loop and call Add repeatedly
                        MethodInfo moveNext, current, getEnumerator =
 GetEnumeratorInfo(ctx.Model, out moveNext, out current);
                        Helpers.DebugAssert(moveNext != null);
                        Helpers.DebugAssert(current != null);
                        Helpers.DebugAssert(getEnumerator != null);

                        Type enumeratorType = getEnumerator.ReturnType;
                        using (Compiler.Local iter = new Compiler.Local(ctx, enumeratorType))
                        {
                            ctx.LoadAddress(oldList, ExpectedType);
                            ctx.EmitCall(getEnumerator);
                            ctx.StoreValue(iter);
                            using (ctx.Using(iter))
                            {
                                Compiler.CodeLabel body = ctx.DefineLabel(), next = ctx.DefineLabel();
                                ctx.Branch(next, false);

                                ctx.MarkLabel(body);
                                ctx.LoadAddress(builder, builder.Type);
                                ctx.LoadAddress(iter, enumeratorType);                                
                                ctx.EmitCall(current);
                                ctx.EmitCall(add);
                                if (add.ReturnType != null && add.ReturnType != voidType) ctx.DiscardValue();

                                ctx.MarkLabel(next);
                                ctx.LoadAddress(iter, enumeratorType);
                                ctx.EmitCall(moveNext);
                                ctx.BranchIfTrue(body, false);
                            }
                        }
                    }


                    ctx.MarkLabel(done);
                }

                EmitReadList(ctx, builder, Tail, add, packedWireType, false);

                ctx.LoadAddress(builder, builder.Type);
                ctx.EmitCall(finish);
                if(ExpectedType != finish.ReturnType)
                {
                    ctx.Cast(ExpectedType);
                }
            }
        }
#endif
    }
}
#endif