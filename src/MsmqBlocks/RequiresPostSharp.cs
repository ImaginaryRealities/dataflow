//-----------------------------------------------------------------------------
// <copyright file="RequiresPostSharp.cs" company="ImaginaryRealities">
// Copyright 2013 ImaginaryRealities, LLC
// </copyright>
//-----------------------------------------------------------------------------

#if !POSTSHARP
#error PostSharp is not introduced in the build process. If NuGet just restored the PostSharp package, you need to rebuild the solution.
#endif