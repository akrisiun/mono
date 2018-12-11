

#   <Compile Include="..\..\..\external\corert\src\Common\src\System\Numerics\Hashing\HashHelpers.cs" />
#    <Compile Include="..\..\..\external\corert\src\System.Private.CoreLib\src\System\Collections\LowLevelComparer.cs" />
#    <Compile Include="..\..\..\external\corert\src\System.Private.CoreLib\src\System\Collections\ObjectEqualityComparer.cs" />

mkdir corert\System\Numerics\Hashing -force
# mkdir corert\System.Private.CoreLib\src\System\Collections

copy-item ..\..\..\external\corert\src\Common\src\System\Numerics\Hashing\HashHelpers.cs `
                                corert\System\Numerics\Hashing\HashHelpers.cs -force -verbose
copy-item ..\..\..\external\corert\src\System.Private.CoreLib\src\System\Collections\LowLevelComparer.cs `
                                corert\System.Private.CoreLib\src\System\Collections\LowLevelComparer.cs -force -verbose
copy-item ..\..\..\external\corert\src\System.Private.CoreLib\src\System\Collections\ObjectEqualityComparer.cs `
                                corert\System.Private.CoreLib\src\System\Collections\ObjectEqualityComparer.cs -force -verbose


#    <Compile Include="..\..\..\external\corert\src\Common\src\System\Numerics\Hashing\HashHelpers.cs" />
#    <Compile Include="..\..\..\external\corert\src\System.Private.CoreLib\src\System\Collections\LowLevelComparer.cs" />
#    <Compile Include="..\..\..\external\corert\src\System.Private.CoreLib\src\System\Collections\ObjectEqualityComparer.cs" />
#    <Compile Include="..\..\..\external\corert\src\System.Private.CoreLib\src\System\Tuple.cs" />
#    <Compile Include="..\..\..\external\corert\src\System.Private.CoreLib\src\System\ValueTuple.cs" />
