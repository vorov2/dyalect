using Dyalect.Compiler;
using Dyalect.Debug;
using System;
namespace Dyalect.Runtime.Types;

public static class Func
{
    public static DyFunction Compose(DyFunction first, DyFunction second) => new CompositionContainer(first, second);

    public static DyFunction Variant(string name, Func<ExecutionContext, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction1(name, fun, pars, varArgIndex, FunAttr.Vari);
    
    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject[], DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction(name, fun, pars, varArgIndex);

    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject> fun) => new MemberFunction0(name, fun);

    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction1(name, fun, pars, varArgIndex);

    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction2(name, fun, pars, varArgIndex);

    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction3(name, fun, pars, varArgIndex);

    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction4(name, fun, pars, varArgIndex);

    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction5(name, fun, pars, varArgIndex);

    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction6(name, fun, pars, varArgIndex);

    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction7(name, fun, pars, varArgIndex);

    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction8(name, fun, pars, varArgIndex);

    //public static DyFunction Static(string name, Func<ExecutionContext, DyObject> fun) => new StaticFunction0(name, fun, Array.Empty<Par>());

    public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction1(name, fun, pars, varArgIndex);

    public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction2(name, fun, pars, varArgIndex);

    public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction3(name, fun, pars, varArgIndex);

    public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction4(name, fun, pars, varArgIndex);

    public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction5(name, fun, pars, varArgIndex);

    public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction6(name, fun, pars, varArgIndex);

    public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction7(name, fun, pars, varArgIndex);

    public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction8(name, fun, pars, varArgIndex);
    
    public static DyFunction Auto(string name, Func<ExecutionContext, DyObject> fun) => new AutoFunction0(name, fun);

    public static DyFunction Auto(string name, Func<ExecutionContext, DyObject, DyObject> fun) => new AutoFunction1(name, fun);


    ///----------------------
    public static DyFunction Constructor<P1>(string name, Func<ExecutionContext, P1, DyObject?> fun, Par par)
        where P1 : DyObject
        => new Constructor<P1>(name, fun!, par);

    public static DyFunction Constructor<P1>(string name, Func<P1, DyObject?> fun, Par par)
        where P1 : DyObject
        => new ConstructorNoContext<P1>(name, fun!, par);

    public static DyFunction Property(string name, Func<ExecutionContext, DyObject> fun) => new StaticProperty(name, fun);

    public static DyFunction Property<P1>(string name, Func<ExecutionContext, P1, DyObject> fun)
        where P1 : DyObject
        => new InstanceProperty<P1>(name, fun!);

    public static DyFunction Instance<P1>(string name, Func<ExecutionContext, P1, DyObject?> fun)
        where P1 : DyObject
        => new InstanceMethod<P1>(name, fun!);

    public static DyFunction Instance<P1, P2>(string name, Func<ExecutionContext, P1, P2, DyObject?> fun, Par par)
        where P1 : DyObject
        where P2 : DyObject
        => new InstanceMethod<P1, P2>(name, fun!, par);

    public static DyFunction Instance<P1, P2, P3>(string name, Func<ExecutionContext, P1, P2, P3, DyObject?> fun, Par par1, Par par2)
        where P1 : DyObject
        where P2 : DyObject
        where P3 : DyObject
    => new InstanceMethod<P1, P2, P3>(name, fun!, par1, par2);

    public static DyFunction Instance<P1, P2, P3, P4>(string name, Func<ExecutionContext, P1, P2, P3, P4, DyObject?> fun, Par par1, Par par2, Par par3)
        where P1 : DyObject
        where P2 : DyObject
        where P3 : DyObject
        where P4 : DyObject
    => new InstanceMethod<P1, P2, P3, P4>(name, fun!, par1, par2, par3);

    public static DyFunction Method<P1, P2, P3, P4, P5>(string name, Func<ExecutionContext, P1, P2, P3, P4, P5, DyObject?> fun, Par par1, Par par2, Par par3, Par par4)
        where P1 : DyObject
        where P2 : DyObject
        where P3 : DyObject
        where P4 : DyObject
        where P5 : DyObject
    => new InstanceMethod<P1, P2, P3, P4, P5>(name, fun!, par1, par2, par3, par4);

    public static DyFunction Instance<P1, P2, P3, P4, P5, P6>(string name, Func<ExecutionContext, P1, P2, P3, P4, P5, P6, DyObject?> fun, Par par1, Par par2, Par par3, Par par4, Par par5)
        where P1 : DyObject
        where P2 : DyObject
        where P3 : DyObject
        where P4 : DyObject
        where P5 : DyObject
        where P6 : DyObject
    => new InstanceMethod<P1, P2, P3, P4, P5, P6>(name, fun!, par1, par2, par3, par4, par5);

    public static DyFunction Instance<P1, P2, P3, P4, P5, P6, P7>(string name, Func<ExecutionContext, P1, P2, P3, P4, P5, P6, P7, DyObject?> fun, Par par1, Par par2, Par par3, Par par4, Par par5, Par par6)
        where P1 : DyObject
        where P2 : DyObject
        where P3 : DyObject
        where P4 : DyObject
        where P5 : DyObject
        where P6 : DyObject
        where P7 : DyObject
    => new InstanceMethod<P1, P2, P3, P4, P5, P6, P7>(name, fun!, par1, par2, par3, par4, par5, par6);

    public static DyFunction Instance<P1, P2, P3, P4, P5, P6, P7, P8>(string name, Func<ExecutionContext, P1, P2, P3, P4, P5, P6, P7, P8, DyObject?> fun, Par par1, Par par2, Par par3, Par par4, Par par5, Par par6, Par par7)
        where P1 : DyObject
        where P2 : DyObject
        where P3 : DyObject
        where P4 : DyObject
        where P5 : DyObject
        where P6 : DyObject
        where P7 : DyObject
        where P8 : DyObject
    => new InstanceMethod<P1, P2, P3, P4, P5, P6, P7, P8>(name, fun!, par1, par2, par3, par4, par5, par6, par7);

    public static DyFunction Instance<P1, P2, P3, P4, P5, P6, P7, P8, P9>(string name, Func<ExecutionContext, P1, P2, P3, P4, P5, P6, P7, P8, P9, DyObject?> fun, Par par1, Par par2, Par par3, Par par4, Par par5, Par par6, Par par7, Par par8)
        where P1 : DyObject
        where P2 : DyObject
        where P3 : DyObject
        where P4 : DyObject
        where P5 : DyObject
        where P6 : DyObject
        where P7 : DyObject
        where P8 : DyObject
        where P9 : DyObject
    => new InstanceMethod<P1, P2, P3, P4, P5, P6, P7, P8, P9>(name, fun!, par1, par2, par3, par4, par5, par6, par7, par8);

    public static DyFunction Static(string name, Func<ExecutionContext, DyObject?> fun)
        => new StaticMethod(name, fun!);

    public static DyFunction Static<P1>(string name, Func<ExecutionContext, P1, DyObject?> fun, Par par)
        where P1 : DyObject
        => new StaticMethod<P1>(name, fun!, par);

    public static DyFunction Static<P1, P2>(string name, Func<ExecutionContext, P1, P2, DyObject?> fun, Par par1, Par par2)
        where P1 : DyObject
        where P2 : DyObject
        => new StaticMethod<P1, P2>(name, fun!, par1, par2);

    public static DyFunction Static<P1, P2, P3>(string name, Func<ExecutionContext, P1, P2, P3, DyObject?> fun, Par par1, Par par2, Par par3)
        where P1 : DyObject
        where P2 : DyObject
        where P3 : DyObject
    => new StaticMethod<P1, P2, P3>(name, fun!, par1, par2, par3);

    public static DyFunction Static<P1, P2, P3, P4>(string name, Func<ExecutionContext, P1, P2, P3, P4, DyObject?> fun, Par par1, Par par2, Par par3, Par par4)
        where P1 : DyObject
        where P2 : DyObject
        where P3 : DyObject
        where P4 : DyObject
    => new StaticMethod<P1, P2, P3, P4>(name, fun!, par1, par2, par3, par4);

    public static DyFunction Static<P1, P2, P3, P4, P5>(string name, Func<ExecutionContext, P1, P2, P3, P4, P5, DyObject?> fun, Par par1, Par par2, Par par3, Par par4, Par par5)
        where P1 : DyObject
        where P2 : DyObject
        where P3 : DyObject
        where P4 : DyObject
        where P5 : DyObject
    => new StaticMethod<P1, P2, P3, P4, P5>(name, fun!, par1, par2, par3, par4, par5);

    public static DyFunction Static<P1, P2, P3, P4, P5, P6>(string name, Func<ExecutionContext, P1, P2, P3, P4, P5, P6, DyObject?> fun, Par par1, Par par2, Par par3, Par par4, Par par5, Par par6)
        where P1 : DyObject
        where P2 : DyObject
        where P3 : DyObject
        where P4 : DyObject
        where P5 : DyObject
        where P6 : DyObject
    => new StaticMethod<P1, P2, P3, P4, P5, P6>(name, fun!, par1, par2, par3, par4, par5, par6);

    public static DyFunction Static<P1, P2, P3, P4, P5, P6, P7>(string name, Func<ExecutionContext, P1, P2, P3, P4, P5, P6, P7, DyObject?> fun, Par par1, Par par2, Par par3, Par par4, Par par5, Par par6, Par par7)
        where P1 : DyObject
        where P2 : DyObject
        where P3 : DyObject
        where P4 : DyObject
        where P5 : DyObject
        where P6 : DyObject
        where P7 : DyObject
    => new StaticMethod<P1, P2, P3, P4, P5, P6, P7>(name, fun!, par1, par2, par3, par4, par5, par6, par7);

    public static DyFunction Static<P1, P2, P3, P4, P5, P6, P7, P8>(string name, Func<ExecutionContext, P1, P2, P3, P4, P5, P6, P7, P8, DyObject?> fun, Par par1, Par par2, Par par3, Par par4, Par par5, Par par6, Par par7, Par par8)
        where P1 : DyObject
        where P2 : DyObject
        where P3 : DyObject
        where P4 : DyObject
        where P5 : DyObject
        where P6 : DyObject
        where P7 : DyObject
        where P8 : DyObject
    => new StaticMethod<P1, P2, P3, P4, P5, P6, P7, P8>(name, fun!, par1, par2, par3, par4, par5, par6, par7, par8);

    public static DyFunction Static<P1, P2, P3, P4, P5, P6, P7, P8, P9>(string name, Func<ExecutionContext, P1, P2, P3, P4, P5, P6, P7, P8, P9, DyObject?> fun, Par par1, Par par2, Par par3, Par par4, Par par5, Par par6, Par par7, Par par8, Par par9)
        where P1 : DyObject
        where P2 : DyObject
        where P3 : DyObject
        where P4 : DyObject
        where P5 : DyObject
        where P6 : DyObject
        where P7 : DyObject
        where P8 : DyObject
        where P9 : DyObject
    => new StaticMethod<P1, P2, P3, P4, P5, P6, P7, P8, P9>(name, fun!, par1, par2, par3, par4, par5, par6, par7, par8, par9);

}

