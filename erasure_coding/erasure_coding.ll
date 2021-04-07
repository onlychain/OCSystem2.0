; ModuleID = '.\erasure_coding.cpp'
source_filename = ".\\erasure_coding.cpp"
target datalayout = "e-m:w-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
target triple = "x86_64-unknown-linux-gnu"

%struct.GF = type { i8 }

$"?value_table@GF@@0QBEB" = comdat any

$"?exp_table@GF@@0QBEB" = comdat any

$"??@d21f2700854ede5ffe0a07e78d081b25@" = comdat any

@"?value_table@GF@@0QBEB" = linkonce_odr dso_local local_unnamed_addr constant [256 x i8] c"\00\01\02\04\08\10 @\80\1D:t\E8\CD\87\13&L\98-Z\B4u\EA\C9\8F\03\06\0C\180`\C0\9D'N\9C%J\945j\D4\B5w\EE\C1\9F#F\8C\05\0A\14(P\A0]\BAi\D2\B9o\DE\A1_\BEa\C2\99/^\BCe\CA\89\0F\1E<x\F0\FD\E7\D3\BBk\D6\B1\7F\FE\E1\DF\A3[\B6q\E2\D9\AFC\86\11\22D\88\0D\1A4h\D0\BDg\CE\81\1F>|\F8\ED\C7\93;v\EC\C5\973f\CC\85\17.\\\B8m\DA\A9O\9E!B\84\15*T\A8M\9A)R\A4U\AAI\929r\E4\D5\B7s\E6\D1\BFc\C6\91?~\FC\E5\D7\B3{\F6\F1\FF\E3\DB\ABK\961b\C4\957n\DC\A5W\AEA\82\192d\C8\8D\07\0E\1C8p\E0\DD\A7S\A6Q\A2Y\B2y\F2\F9\EF\C3\9B+V\ACE\8A\09\12$H\90=z\F4\F5\F7\F3\FB\EB\CB\8B\0B\16,X\B0}\FA\E9\CF\83\1B6l\D8\ADG\8E", comdat, align 16
@"?exp_table@GF@@0QBEB" = linkonce_odr dso_local local_unnamed_addr constant [256 x i8] c"\FF\00\01\19\022\1A\C6\03\DF3\EE\1Bh\C7K\04d\E0\0E4\8D\EF\81\1C\C1i\F8\C8\08Lq\05\8Ae/\E1$\0F!5\93\8E\DA\F0\12\82E\1D\B5\C2}j'\F9\B9\C9\9A\09xM\E4r\A6\06\BF\8Bbf\DD0\FD\E2\98%\B3\10\91\22\886\D0\94\CE\8F\96\DB\BD\F1\D2\13\\\838F@\1EB\B6\A3\C3H~nk:(T\FA\85\BA=\CA^\9B\9F\0A\15y+N\D4\E5\ACs\F3\A7W\07p\C0\F7\8C\80c\0DgJ\DE\ED1\C5\FE\18\E3\A5\99w&\B8\B4|\11D\92\D9# \89.7?\D1[\95\BC\CF\CD\90\87\97\B2\DC\FC\BEa\F2V\D3\AB\14*]\9E\84<9SGmA\A2\1F-C\D8\B7{\A4v\C4\17I\EC\7F\0Co\F6l\A1;R)\9DU\AA\FB`\86\B1\BB\CC>Z\CBY_\B0\9C\A9\A0Q\0B\F5\16\EBzu,\D7O\AE\D5\E9\E6\E7\AD\E8t\D6\F4\EA\A8PX\AF", comdat, align 16

; Function Attrs: nounwind uwtable
define dso_local i32 @ec_encode(i8* noalias nocapture readonly %0, i32 %1, i8* noalias nocapture %2, i32 %3, i32 %4, i32 %5) local_unnamed_addr #0 {
  %7 = add i32 %4, -1
  %8 = icmp ugt i32 %7, 253
  br i1 %8, label %168, label %9

9:                                                ; preds = %6
  %10 = add i32 %5, -1
  %11 = icmp ugt i32 %10, 253
  br i1 %11, label %168, label %12

12:                                               ; preds = %9
  %13 = srem i32 %1, %4
  %14 = sdiv i32 %1, %4
  %15 = icmp eq i32 %13, 0
  br i1 %15, label %16, label %168

16:                                               ; preds = %12
  %17 = srem i32 %3, %5
  %18 = sdiv i32 %3, %5
  %19 = icmp eq i32 %17, 0
  br i1 %19, label %20, label %168

20:                                               ; preds = %16
  %21 = icmp eq i32 %14, %18
  br i1 %21, label %22, label %168

22:                                               ; preds = %20
  %23 = mul nuw nsw i32 %5, %4
  %24 = zext i32 %23 to i64
  %25 = tail call i8* @llvm.stacksave()
  %26 = alloca %struct.GF*, i64 %24, align 16
  %27 = icmp eq i32 %4, 1
  br label %39

28:                                               ; preds = %53
  %29 = icmp sgt i32 %14, 0
  br i1 %29, label %30, label %85

30:                                               ; preds = %28
  %31 = icmp sgt i32 %4, 0
  %32 = sext i32 %4 to i64
  %33 = zext i32 %4 to i64
  %34 = add nsw i64 %33, -1
  %35 = and i64 %33, 3
  %36 = icmp ult i64 %34, 3
  %37 = and i64 %33, 4294967292
  %38 = icmp eq i64 %35, 0
  br label %79

39:                                               ; preds = %53, %22
  %40 = phi i64 [ 0, %22 ], [ %54, %53 ]
  %41 = phi i32 [ 0, %22 ], [ %55, %53 ]
  %42 = urem i32 %41, 255
  %43 = add nuw nsw i32 %42, 1
  %44 = zext i32 %43 to i64
  %45 = getelementptr inbounds [256 x i8], [256 x i8]* @"?value_table@GF@@0QBEB", i64 0, i64 %44
  %46 = load i8, i8* %45, align 1, !tbaa !3, !noalias !6
  %47 = zext i8 %46 to i64
  %48 = getelementptr inbounds [256 x i8], [256 x i8]* @"?exp_table@GF@@0QBEB", i64 0, i64 %47
  %49 = shl i64 %40, 32
  %50 = ashr exact i64 %49, 32
  %51 = getelementptr inbounds %struct.GF*, %struct.GF** %26, i64 %50
  store %struct.GF* getelementptr inbounds ([65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 256), %struct.GF** %51, align 8, !tbaa !9
  tail call void @llvm.prefetch.p0i8(i8* nonnull getelementptr inbounds ([65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 256, i32 0), i32 0, i32 0, i32 1)
  tail call void @llvm.prefetch.p0i8(i8* nonnull getelementptr inbounds ([65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 320, i32 0), i32 0, i32 0, i32 1)
  tail call void @llvm.prefetch.p0i8(i8* nonnull getelementptr inbounds ([65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 384, i32 0), i32 0, i32 0, i32 1)
  tail call void @llvm.prefetch.p0i8(i8* nonnull getelementptr inbounds ([65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 448, i32 0), i32 0, i32 0, i32 1)
  %52 = add nsw i64 %50, 1
  br i1 %27, label %53, label %57

53:                                               ; preds = %57, %39
  %54 = phi i64 [ %52, %39 ], [ %77, %57 ]
  %55 = add nuw nsw i32 %41, 1
  %56 = icmp eq i32 %55, %5
  br i1 %56, label %28, label %39

57:                                               ; preds = %39, %57
  %58 = phi i64 [ %77, %57 ], [ %52, %39 ]
  %59 = phi i32 [ %76, %57 ], [ 1, %39 ]
  %60 = load i8, i8* %48, align 1, !tbaa !3, !noalias !11
  %61 = zext i8 %60 to i32
  %62 = mul nsw i32 %59, %61
  %63 = urem i32 %62, 255
  %64 = add nuw nsw i32 %63, 1
  %65 = zext i32 %64 to i64
  %66 = getelementptr inbounds [256 x i8], [256 x i8]* @"?value_table@GF@@0QBEB", i64 0, i64 %65
  %67 = load i8, i8* %66, align 1, !tbaa !3, !noalias !14
  %68 = zext i8 %67 to i64
  %69 = shl nuw nsw i64 %68, 8
  %70 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %69
  %71 = getelementptr inbounds %struct.GF*, %struct.GF** %26, i64 %58
  store %struct.GF* %70, %struct.GF** %71, align 8, !tbaa !9
  %72 = getelementptr inbounds %struct.GF, %struct.GF* %70, i64 0, i32 0
  tail call void @llvm.prefetch.p0i8(i8* nonnull %72, i32 0, i32 0, i32 1)
  %73 = getelementptr inbounds %struct.GF, %struct.GF* %70, i64 64, i32 0
  tail call void @llvm.prefetch.p0i8(i8* nonnull %73, i32 0, i32 0, i32 1)
  %74 = getelementptr inbounds %struct.GF, %struct.GF* %70, i64 128, i32 0
  tail call void @llvm.prefetch.p0i8(i8* nonnull %74, i32 0, i32 0, i32 1)
  %75 = getelementptr inbounds %struct.GF, %struct.GF* %70, i64 192, i32 0
  tail call void @llvm.prefetch.p0i8(i8* nonnull %75, i32 0, i32 0, i32 1)
  %76 = add nuw nsw i32 %59, 1
  %77 = add nsw i64 %58, 1
  %78 = icmp eq i32 %76, %4
  br i1 %78, label %53, label %57, !llvm.loop !17

79:                                               ; preds = %30, %91
  %80 = phi i64 [ 0, %30 ], [ %93, %91 ]
  %81 = phi i64 [ 0, %30 ], [ %117, %91 ]
  %82 = phi i32 [ 0, %30 ], [ %92, %91 ]
  %83 = shl i64 %81, 32
  %84 = ashr exact i64 %83, 32
  br label %86

85:                                               ; preds = %91, %28
  tail call void @llvm.stackrestore(i8* %25)
  br label %168

86:                                               ; preds = %115, %79
  %87 = phi i64 [ %84, %79 ], [ %117, %115 ]
  %88 = phi i32 [ 0, %79 ], [ %119, %115 ]
  %89 = phi %struct.GF** [ %26, %79 ], [ %120, %115 ]
  br i1 %31, label %90, label %115

90:                                               ; preds = %86
  br i1 %36, label %95, label %122

91:                                               ; preds = %115
  %92 = add nuw nsw i32 %82, 1
  %93 = add nsw i64 %80, %32
  %94 = icmp eq i32 %92, %14
  br i1 %94, label %85, label %79

95:                                               ; preds = %122, %90
  %96 = phi i8 [ undef, %90 ], [ %164, %122 ]
  %97 = phi i64 [ 0, %90 ], [ %165, %122 ]
  %98 = phi i8 [ 0, %90 ], [ %164, %122 ]
  br i1 %38, label %115, label %99

99:                                               ; preds = %95, %99
  %100 = phi i64 [ %112, %99 ], [ %97, %95 ]
  %101 = phi i8 [ %111, %99 ], [ %98, %95 ]
  %102 = phi i64 [ %113, %99 ], [ %35, %95 ]
  %103 = getelementptr inbounds %struct.GF*, %struct.GF** %89, i64 %100
  %104 = load %struct.GF*, %struct.GF** %103, align 8, !tbaa !9
  %105 = add nsw i64 %100, %80
  %106 = getelementptr inbounds i8, i8* %0, i64 %105
  %107 = load i8, i8* %106, align 1, !tbaa !3
  %108 = zext i8 %107 to i64
  %109 = getelementptr inbounds %struct.GF, %struct.GF* %104, i64 %108, i32 0
  %110 = load i8, i8* %109, align 1, !tbaa.struct !19
  %111 = xor i8 %110, %101
  %112 = add nuw nsw i64 %100, 1
  %113 = add i64 %102, -1
  %114 = icmp eq i64 %113, 0
  br i1 %114, label %115, label %99, !llvm.loop !20

115:                                              ; preds = %95, %99, %86
  %116 = phi i8 [ 0, %86 ], [ %96, %95 ], [ %111, %99 ]
  %117 = add nsw i64 %87, 1
  %118 = getelementptr inbounds i8, i8* %2, i64 %87
  store i8 %116, i8* %118, align 1, !tbaa !3
  %119 = add nuw nsw i32 %88, 1
  %120 = getelementptr inbounds %struct.GF*, %struct.GF** %89, i64 %32
  %121 = icmp eq i32 %119, %5
  br i1 %121, label %91, label %86

122:                                              ; preds = %90, %122
  %123 = phi i64 [ %165, %122 ], [ 0, %90 ]
  %124 = phi i8 [ %164, %122 ], [ 0, %90 ]
  %125 = phi i64 [ %166, %122 ], [ %37, %90 ]
  %126 = getelementptr inbounds %struct.GF*, %struct.GF** %89, i64 %123
  %127 = load %struct.GF*, %struct.GF** %126, align 8, !tbaa !9
  %128 = add nsw i64 %123, %80
  %129 = getelementptr inbounds i8, i8* %0, i64 %128
  %130 = load i8, i8* %129, align 1, !tbaa !3
  %131 = zext i8 %130 to i64
  %132 = getelementptr inbounds %struct.GF, %struct.GF* %127, i64 %131, i32 0
  %133 = load i8, i8* %132, align 1, !tbaa.struct !19
  %134 = xor i8 %133, %124
  %135 = or i64 %123, 1
  %136 = getelementptr inbounds %struct.GF*, %struct.GF** %89, i64 %135
  %137 = load %struct.GF*, %struct.GF** %136, align 8, !tbaa !9
  %138 = add nsw i64 %135, %80
  %139 = getelementptr inbounds i8, i8* %0, i64 %138
  %140 = load i8, i8* %139, align 1, !tbaa !3
  %141 = zext i8 %140 to i64
  %142 = getelementptr inbounds %struct.GF, %struct.GF* %137, i64 %141, i32 0
  %143 = load i8, i8* %142, align 1, !tbaa.struct !19
  %144 = xor i8 %143, %134
  %145 = or i64 %123, 2
  %146 = getelementptr inbounds %struct.GF*, %struct.GF** %89, i64 %145
  %147 = load %struct.GF*, %struct.GF** %146, align 8, !tbaa !9
  %148 = add nsw i64 %145, %80
  %149 = getelementptr inbounds i8, i8* %0, i64 %148
  %150 = load i8, i8* %149, align 1, !tbaa !3
  %151 = zext i8 %150 to i64
  %152 = getelementptr inbounds %struct.GF, %struct.GF* %147, i64 %151, i32 0
  %153 = load i8, i8* %152, align 1, !tbaa.struct !19
  %154 = xor i8 %153, %144
  %155 = or i64 %123, 3
  %156 = getelementptr inbounds %struct.GF*, %struct.GF** %89, i64 %155
  %157 = load %struct.GF*, %struct.GF** %156, align 8, !tbaa !9
  %158 = add nsw i64 %155, %80
  %159 = getelementptr inbounds i8, i8* %0, i64 %158
  %160 = load i8, i8* %159, align 1, !tbaa !3
  %161 = zext i8 %160 to i64
  %162 = getelementptr inbounds %struct.GF, %struct.GF* %157, i64 %161, i32 0
  %163 = load i8, i8* %162, align 1, !tbaa.struct !19
  %164 = xor i8 %163, %154
  %165 = add nuw nsw i64 %123, 4
  %166 = add i64 %125, -4
  %167 = icmp eq i64 %166, 0
  br i1 %167, label %95, label %122

168:                                              ; preds = %85, %20, %16, %12, %9, %6
  %169 = phi i32 [ 1, %6 ], [ 2, %9 ], [ 3, %12 ], [ 4, %16 ], [ 0, %85 ], [ 5, %20 ]
  ret i32 %169
}

; Function Attrs: argmemonly nounwind willreturn
declare void @llvm.lifetime.start.p0i8(i64 immarg, i8* nocapture) #1

; Function Attrs: nounwind
declare i8* @llvm.stacksave() #2

; Function Attrs: argmemonly nounwind willreturn
declare void @llvm.lifetime.end.p0i8(i64 immarg, i8* nocapture) #1

; Function Attrs: inaccessiblemem_or_argmemonly nounwind willreturn
declare void @llvm.prefetch.p0i8(i8* nocapture readonly, i32 immarg, i32 immarg, i32) #3

; Function Attrs: argmemonly nounwind willreturn
declare void @llvm.memcpy.p0i8.p0i8.i64(i8* noalias nocapture writeonly, i8* noalias nocapture readonly, i64, i1 immarg) #1

; Function Attrs: nounwind
declare void @llvm.stackrestore(i8*) #2

; Function Attrs: nounwind uwtable
define dso_local i32 @ec_decode(i8* noalias nocapture %0, i32 %1, i32 %2, [2 x i32]* noalias nocapture readonly %3, i32 %4) local_unnamed_addr #0 personality i32 (...)* @__CxxFrameHandler3 {
  %6 = alloca [32 x i8], align 16
  %7 = add i32 %2, -1
  %8 = icmp ugt i32 %7, 253
  br i1 %8, label %391, label %9

9:                                                ; preds = %5
  %10 = add i32 %4, -1
  %11 = icmp ugt i32 %10, 253
  br i1 %11, label %391, label %12

12:                                               ; preds = %9
  %13 = srem i32 %1, %2
  %14 = sdiv i32 %1, %2
  %15 = icmp eq i32 %13, 0
  br i1 %15, label %16, label %391

16:                                               ; preds = %12
  %17 = zext i32 %4 to i64
  br label %20

18:                                               ; preds = %27
  %19 = icmp eq i64 %31, %17
  br i1 %19, label %32, label %20

20:                                               ; preds = %18, %16
  %21 = phi i64 [ 0, %16 ], [ %31, %18 ]
  %22 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %21, i64 0
  %23 = load i32, i32* %22, align 4, !tbaa !22
  %24 = icmp sgt i32 %23, -1
  %25 = icmp slt i32 %23, %2
  %26 = and i1 %24, %25
  br i1 %26, label %27, label %391

27:                                               ; preds = %20
  %28 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %21, i64 1
  %29 = load i32, i32* %28, align 4, !tbaa !22
  %30 = icmp ugt i32 %29, 253
  %31 = add nuw nsw i64 %21, 1
  br i1 %30, label %391, label %18

32:                                               ; preds = %18
  %33 = getelementptr inbounds [32 x i8], [32 x i8]* %6, i64 0, i64 0
  call void @llvm.lifetime.start.p0i8(i64 32, i8* nonnull %33) #2
  call void @llvm.memset.p0i8.i64(i8* nonnull align 16 dereferenceable(32) %33, i8 0, i64 32, i1 false) #2
  %34 = zext i32 %4 to i64
  br label %35

35:                                               ; preds = %48, %32
  %36 = phi i64 [ 0, %32 ], [ %53, %48 ]
  %37 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %36, i64 0
  %38 = load i32, i32* %37, align 4, !tbaa !22
  %39 = sext i32 %38 to i64
  %40 = lshr i64 %39, 3
  %41 = getelementptr inbounds [32 x i8], [32 x i8]* %6, i64 0, i64 %40
  %42 = load i8, i8* %41, align 1, !tbaa !3
  %43 = zext i8 %42 to i32
  %44 = and i32 %38, 7
  %45 = shl nuw nsw i32 1, %44
  %46 = and i32 %45, %43
  %47 = icmp eq i32 %46, 0
  br i1 %47, label %48, label %55

48:                                               ; preds = %35
  %49 = trunc i32 %38 to i8
  %50 = and i8 %49, 7
  %51 = shl nuw i8 1, %50
  %52 = or i8 %51, %42
  store i8 %52, i8* %41, align 1, !tbaa !3
  %53 = add nuw nsw i64 %36, 1
  %54 = icmp eq i64 %53, %34
  br i1 %54, label %56, label %35

55:                                               ; preds = %35
  call void @llvm.lifetime.end.p0i8(i64 32, i8* nonnull %33) #2
  br label %391

56:                                               ; preds = %48
  call void @llvm.lifetime.end.p0i8(i64 32, i8* nonnull %33) #2
  call void @llvm.lifetime.start.p0i8(i64 32, i8* nonnull %33) #2
  call void @llvm.memset.p0i8.i64(i8* nonnull align 16 dereferenceable(32) %33, i8 0, i64 32, i1 false) #2
  br label %57

57:                                               ; preds = %70, %56
  %58 = phi i64 [ 0, %56 ], [ %75, %70 ]
  %59 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %58, i64 1
  %60 = load i32, i32* %59, align 4, !tbaa !22
  %61 = sext i32 %60 to i64
  %62 = lshr i64 %61, 3
  %63 = getelementptr inbounds [32 x i8], [32 x i8]* %6, i64 0, i64 %62
  %64 = load i8, i8* %63, align 1, !tbaa !3
  %65 = zext i8 %64 to i32
  %66 = and i32 %60, 7
  %67 = shl nuw nsw i32 1, %66
  %68 = and i32 %67, %65
  %69 = icmp eq i32 %68, 0
  br i1 %69, label %70, label %77

70:                                               ; preds = %57
  %71 = trunc i32 %60 to i8
  %72 = and i8 %71, 7
  %73 = shl nuw i8 1, %72
  %74 = or i8 %73, %64
  store i8 %74, i8* %63, align 1, !tbaa !3
  %75 = add nuw nsw i64 %58, 1
  %76 = icmp eq i64 %75, %34
  br i1 %76, label %78, label %57

77:                                               ; preds = %57
  call void @llvm.lifetime.end.p0i8(i64 32, i8* nonnull %33) #2
  br label %391

78:                                               ; preds = %70
  call void @llvm.lifetime.end.p0i8(i64 32, i8* nonnull %33) #2
  %79 = mul nsw i32 %2, %2
  %80 = zext i32 %79 to i64
  %81 = tail call i8* @llvm.stacksave()
  %82 = alloca %struct.GF, i64 %80, align 16
  %83 = getelementptr inbounds %struct.GF, %struct.GF* %82, i64 0, i32 0
  call void @llvm.memset.p0i8.i64(i8* nonnull align 16 %83, i8 0, i64 %80, i1 false)
  %84 = icmp sgt i32 %2, 0
  br i1 %84, label %85, label %105

85:                                               ; preds = %78
  %86 = zext i32 %2 to i64
  %87 = zext i32 %2 to i64
  %88 = add nsw i64 %87, -1
  %89 = and i64 %87, 3
  %90 = icmp ult i64 %88, 3
  br i1 %90, label %93, label %91

91:                                               ; preds = %85
  %92 = and i64 %87, 4294967292
  br label %109

93:                                               ; preds = %109, %85
  %94 = phi i64 [ 0, %85 ], [ %127, %109 ]
  %95 = icmp eq i64 %89, 0
  br i1 %95, label %105, label %96

96:                                               ; preds = %93, %96
  %97 = phi i64 [ %102, %96 ], [ %94, %93 ]
  %98 = phi i64 [ %103, %96 ], [ %89, %93 ]
  %99 = mul nsw i64 %97, %86
  %100 = add nuw nsw i64 %99, %97
  %101 = getelementptr inbounds %struct.GF, %struct.GF* %82, i64 %100, i32 0
  store i8 1, i8* %101, align 1, !tbaa !3
  %102 = add nuw nsw i64 %97, 1
  %103 = add i64 %98, -1
  %104 = icmp eq i64 %103, 0
  br i1 %104, label %105, label %96, !llvm.loop !24

105:                                              ; preds = %93, %96, %78
  %106 = zext i32 %4 to i64
  %107 = zext i32 %2 to i64
  %108 = icmp eq i32 %2, 1
  br label %141

109:                                              ; preds = %109, %91
  %110 = phi i64 [ 0, %91 ], [ %127, %109 ]
  %111 = phi i64 [ %92, %91 ], [ %128, %109 ]
  %112 = mul nsw i64 %110, %86
  %113 = add nuw nsw i64 %112, %110
  %114 = getelementptr inbounds %struct.GF, %struct.GF* %82, i64 %113, i32 0
  store i8 1, i8* %114, align 4, !tbaa !3
  %115 = or i64 %110, 1
  %116 = mul nsw i64 %115, %86
  %117 = add nuw nsw i64 %116, %115
  %118 = getelementptr inbounds %struct.GF, %struct.GF* %82, i64 %117, i32 0
  store i8 1, i8* %118, align 1, !tbaa !3
  %119 = or i64 %110, 2
  %120 = mul nsw i64 %119, %86
  %121 = add nuw nsw i64 %120, %119
  %122 = getelementptr inbounds %struct.GF, %struct.GF* %82, i64 %121, i32 0
  store i8 1, i8* %122, align 2, !tbaa !3
  %123 = or i64 %110, 3
  %124 = mul nsw i64 %123, %86
  %125 = add nuw nsw i64 %124, %123
  %126 = getelementptr inbounds %struct.GF, %struct.GF* %82, i64 %125, i32 0
  store i8 1, i8* %126, align 1, !tbaa !3
  %127 = add nuw nsw i64 %110, 4
  %128 = add i64 %111, -4
  %129 = icmp eq i64 %128, 0
  br i1 %129, label %93, label %109

130:                                              ; preds = %162
  call fastcc void @"?mat_inverse@@YAXPEAUGF@@H@Z"(%struct.GF* nonnull %82, i32 %2) #2
  %131 = mul nsw i32 %4, %2
  %132 = zext i32 %131 to i64
  %133 = alloca %struct.GF*, i64 %132, align 16
  %134 = sext i32 %2 to i64
  %135 = zext i32 %4 to i64
  %136 = zext i32 %2 to i64
  %137 = and i64 %136, 1
  %138 = icmp eq i32 %2, 1
  %139 = and i64 %136, 4294967294
  %140 = icmp eq i64 %137, 0
  br label %180

141:                                              ; preds = %162, %105
  %142 = phi i64 [ 0, %105 ], [ %163, %162 ]
  %143 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %142, i64 0
  %144 = load i32, i32* %143, align 4, !tbaa !22
  %145 = mul nsw i32 %144, %2
  %146 = sext i32 %145 to i64
  %147 = getelementptr inbounds %struct.GF, %struct.GF* %82, i64 %146
  %148 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %142, i64 1
  %149 = load i32, i32* %148, align 4, !tbaa !22
  %150 = srem i32 %149, 255
  br i1 %84, label %151, label %162

151:                                              ; preds = %141
  %152 = icmp slt i32 %150, 0
  %153 = add nsw i32 %150, 255
  %154 = select i1 %152, i32 %153, i32 %150
  %155 = add nuw nsw i32 %154, 1
  %156 = zext i32 %155 to i64
  %157 = getelementptr inbounds [256 x i8], [256 x i8]* @"?value_table@GF@@0QBEB", i64 0, i64 %156
  %158 = load i8, i8* %157, align 1, !tbaa !3, !noalias !25
  %159 = zext i8 %158 to i64
  %160 = getelementptr inbounds [256 x i8], [256 x i8]* @"?exp_table@GF@@0QBEB", i64 0, i64 %159
  %161 = getelementptr %struct.GF, %struct.GF* %147, i64 0, i32 0
  store i8 1, i8* %161, align 1, !tbaa !3
  br i1 %108, label %162, label %165

162:                                              ; preds = %151, %165, %141
  %163 = add nuw nsw i64 %142, 1
  %164 = icmp eq i64 %163, %106
  br i1 %164, label %130, label %141

165:                                              ; preds = %151, %165
  %166 = phi i64 [ %178, %165 ], [ 1, %151 ]
  %167 = load i8, i8* %160, align 1, !tbaa !3, !noalias !28
  %168 = zext i8 %167 to i32
  %169 = trunc i64 %166 to i32
  %170 = mul nsw i32 %169, %168
  %171 = urem i32 %170, 255
  %172 = add nuw nsw i32 %171, 1
  %173 = zext i32 %172 to i64
  %174 = getelementptr inbounds [256 x i8], [256 x i8]* @"?value_table@GF@@0QBEB", i64 0, i64 %173
  %175 = load i8, i8* %174, align 1, !tbaa !3, !noalias !31
  %176 = getelementptr inbounds %struct.GF, %struct.GF* %147, i64 %166
  %177 = getelementptr %struct.GF, %struct.GF* %176, i64 0, i32 0
  store i8 %175, i8* %177, align 1, !tbaa !3
  %178 = add nuw nsw i64 %166, 1
  %179 = icmp eq i64 %178, %107
  br i1 %179, label %162, label %165, !llvm.loop !34

180:                                              ; preds = %207, %130
  %181 = phi i64 [ 0, %130 ], [ %208, %207 ]
  br i1 %84, label %182, label %207

182:                                              ; preds = %180
  %183 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %181, i64 0
  %184 = load i32, i32* %183, align 4, !tbaa !22
  %185 = mul nsw i32 %184, %2
  %186 = mul nsw i64 %181, %134
  %187 = sext i32 %185 to i64
  br i1 %138, label %192, label %210

188:                                              ; preds = %207
  %189 = alloca %struct.GF, i64 %135, align 16
  %190 = getelementptr inbounds %struct.GF, %struct.GF* %189, i64 0, i32 0
  call void @llvm.memset.p0i8.i64(i8* nonnull align 16 %190, i8 0, i64 %135, i1 false)
  %191 = icmp sgt i32 %14, 0
  br i1 %191, label %241, label %257

192:                                              ; preds = %210, %182
  %193 = phi i64 [ 0, %182 ], [ %238, %210 ]
  br i1 %140, label %207, label %194

194:                                              ; preds = %192
  %195 = add nsw i64 %193, %187
  %196 = getelementptr inbounds %struct.GF, %struct.GF* %82, i64 %195, i32 0
  %197 = load i8, i8* %196, align 1, !tbaa !35
  %198 = zext i8 %197 to i64
  %199 = shl nuw nsw i64 %198, 8
  %200 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %199
  %201 = add nsw i64 %193, %186
  %202 = getelementptr inbounds %struct.GF*, %struct.GF** %133, i64 %201
  store %struct.GF* %200, %struct.GF** %202, align 8, !tbaa !9
  %203 = getelementptr inbounds %struct.GF, %struct.GF* %200, i64 0, i32 0
  call void @llvm.prefetch.p0i8(i8* nonnull %203, i32 0, i32 0, i32 1)
  %204 = getelementptr inbounds %struct.GF, %struct.GF* %200, i64 64, i32 0
  call void @llvm.prefetch.p0i8(i8* nonnull %204, i32 0, i32 0, i32 1)
  %205 = getelementptr inbounds %struct.GF, %struct.GF* %200, i64 128, i32 0
  call void @llvm.prefetch.p0i8(i8* nonnull %205, i32 0, i32 0, i32 1)
  %206 = getelementptr inbounds %struct.GF, %struct.GF* %200, i64 192, i32 0
  call void @llvm.prefetch.p0i8(i8* nonnull %206, i32 0, i32 0, i32 1)
  br label %207

207:                                              ; preds = %194, %192, %180
  %208 = add nuw nsw i64 %181, 1
  %209 = icmp eq i64 %208, %135
  br i1 %209, label %188, label %180

210:                                              ; preds = %182, %210
  %211 = phi i64 [ %238, %210 ], [ 0, %182 ]
  %212 = phi i64 [ %239, %210 ], [ %139, %182 ]
  %213 = add nsw i64 %211, %187
  %214 = getelementptr inbounds %struct.GF, %struct.GF* %82, i64 %213, i32 0
  %215 = load i8, i8* %214, align 1, !tbaa !35
  %216 = zext i8 %215 to i64
  %217 = shl nuw nsw i64 %216, 8
  %218 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %217
  %219 = add nsw i64 %211, %186
  %220 = getelementptr inbounds %struct.GF*, %struct.GF** %133, i64 %219
  store %struct.GF* %218, %struct.GF** %220, align 8, !tbaa !9
  %221 = getelementptr inbounds %struct.GF, %struct.GF* %218, i64 0, i32 0
  call void @llvm.prefetch.p0i8(i8* nonnull %221, i32 0, i32 0, i32 1)
  %222 = getelementptr inbounds %struct.GF, %struct.GF* %218, i64 64, i32 0
  call void @llvm.prefetch.p0i8(i8* nonnull %222, i32 0, i32 0, i32 1)
  %223 = getelementptr inbounds %struct.GF, %struct.GF* %218, i64 128, i32 0
  call void @llvm.prefetch.p0i8(i8* nonnull %223, i32 0, i32 0, i32 1)
  %224 = getelementptr inbounds %struct.GF, %struct.GF* %218, i64 192, i32 0
  call void @llvm.prefetch.p0i8(i8* nonnull %224, i32 0, i32 0, i32 1)
  %225 = or i64 %211, 1
  %226 = add nsw i64 %225, %187
  %227 = getelementptr inbounds %struct.GF, %struct.GF* %82, i64 %226, i32 0
  %228 = load i8, i8* %227, align 1, !tbaa !35
  %229 = zext i8 %228 to i64
  %230 = shl nuw nsw i64 %229, 8
  %231 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %230
  %232 = add nsw i64 %225, %186
  %233 = getelementptr inbounds %struct.GF*, %struct.GF** %133, i64 %232
  store %struct.GF* %231, %struct.GF** %233, align 8, !tbaa !9
  %234 = getelementptr inbounds %struct.GF, %struct.GF* %231, i64 0, i32 0
  call void @llvm.prefetch.p0i8(i8* nonnull %234, i32 0, i32 0, i32 1)
  %235 = getelementptr inbounds %struct.GF, %struct.GF* %231, i64 64, i32 0
  call void @llvm.prefetch.p0i8(i8* nonnull %235, i32 0, i32 0, i32 1)
  %236 = getelementptr inbounds %struct.GF, %struct.GF* %231, i64 128, i32 0
  call void @llvm.prefetch.p0i8(i8* nonnull %236, i32 0, i32 0, i32 1)
  %237 = getelementptr inbounds %struct.GF, %struct.GF* %231, i64 192, i32 0
  call void @llvm.prefetch.p0i8(i8* nonnull %237, i32 0, i32 0, i32 1)
  %238 = add nuw nsw i64 %211, 2
  %239 = add i64 %212, -2
  %240 = icmp eq i64 %239, 0
  br i1 %240, label %192, label %210

241:                                              ; preds = %188
  %242 = sext i32 %2 to i64
  %243 = zext i32 %2 to i64
  %244 = add nsw i64 %243, -1
  %245 = add nsw i64 %135, -1
  %246 = and i64 %243, 3
  %247 = icmp ult i64 %244, 3
  %248 = and i64 %243, 4294967292
  %249 = icmp eq i64 %246, 0
  %250 = and i64 %135, 3
  %251 = icmp ult i64 %245, 3
  %252 = and i64 %135, 4294967292
  %253 = icmp eq i64 %250, 0
  br label %254

254:                                              ; preds = %241, %350
  %255 = phi i64 [ 0, %241 ], [ %352, %350 ]
  %256 = phi i32 [ 0, %241 ], [ %351, %350 ]
  br label %258

257:                                              ; preds = %350, %188
  call void @llvm.stackrestore(i8* %81)
  br label %391

258:                                              ; preds = %282, %254
  %259 = phi i64 [ 0, %254 ], [ %285, %282 ]
  %260 = phi %struct.GF** [ %133, %254 ], [ %286, %282 ]
  br i1 %84, label %261, label %282

261:                                              ; preds = %258
  br i1 %247, label %262, label %289

262:                                              ; preds = %289, %261
  %263 = phi i8 [ undef, %261 ], [ %331, %289 ]
  %264 = phi i64 [ 0, %261 ], [ %332, %289 ]
  %265 = phi i8 [ 0, %261 ], [ %331, %289 ]
  br i1 %249, label %282, label %266

266:                                              ; preds = %262, %266
  %267 = phi i64 [ %279, %266 ], [ %264, %262 ]
  %268 = phi i8 [ %278, %266 ], [ %265, %262 ]
  %269 = phi i64 [ %280, %266 ], [ %246, %262 ]
  %270 = getelementptr inbounds %struct.GF*, %struct.GF** %260, i64 %267
  %271 = load %struct.GF*, %struct.GF** %270, align 8, !tbaa !9
  %272 = add nsw i64 %267, %255
  %273 = getelementptr inbounds i8, i8* %0, i64 %272
  %274 = load i8, i8* %273, align 1, !tbaa !3
  %275 = zext i8 %274 to i64
  %276 = getelementptr inbounds %struct.GF, %struct.GF* %271, i64 %275, i32 0
  %277 = load i8, i8* %276, align 1, !tbaa.struct !19
  %278 = xor i8 %277, %268
  %279 = add nuw nsw i64 %267, 1
  %280 = add i64 %269, -1
  %281 = icmp eq i64 %280, 0
  br i1 %281, label %282, label %266, !llvm.loop !37

282:                                              ; preds = %262, %266, %258
  %283 = phi i8 [ 0, %258 ], [ %263, %262 ], [ %278, %266 ]
  %284 = getelementptr inbounds %struct.GF, %struct.GF* %189, i64 %259, i32 0
  store i8 %283, i8* %284, align 1, !tbaa !3
  %285 = add nuw nsw i64 %259, 1
  %286 = getelementptr inbounds %struct.GF*, %struct.GF** %260, i64 %242
  %287 = icmp eq i64 %285, %135
  br i1 %287, label %288, label %258

288:                                              ; preds = %282
  br i1 %251, label %335, label %354

289:                                              ; preds = %261, %289
  %290 = phi i64 [ %332, %289 ], [ 0, %261 ]
  %291 = phi i8 [ %331, %289 ], [ 0, %261 ]
  %292 = phi i64 [ %333, %289 ], [ %248, %261 ]
  %293 = getelementptr inbounds %struct.GF*, %struct.GF** %260, i64 %290
  %294 = load %struct.GF*, %struct.GF** %293, align 8, !tbaa !9
  %295 = add nsw i64 %290, %255
  %296 = getelementptr inbounds i8, i8* %0, i64 %295
  %297 = load i8, i8* %296, align 1, !tbaa !3
  %298 = zext i8 %297 to i64
  %299 = getelementptr inbounds %struct.GF, %struct.GF* %294, i64 %298, i32 0
  %300 = load i8, i8* %299, align 1, !tbaa.struct !19
  %301 = xor i8 %300, %291
  %302 = or i64 %290, 1
  %303 = getelementptr inbounds %struct.GF*, %struct.GF** %260, i64 %302
  %304 = load %struct.GF*, %struct.GF** %303, align 8, !tbaa !9
  %305 = add nsw i64 %302, %255
  %306 = getelementptr inbounds i8, i8* %0, i64 %305
  %307 = load i8, i8* %306, align 1, !tbaa !3
  %308 = zext i8 %307 to i64
  %309 = getelementptr inbounds %struct.GF, %struct.GF* %304, i64 %308, i32 0
  %310 = load i8, i8* %309, align 1, !tbaa.struct !19
  %311 = xor i8 %310, %301
  %312 = or i64 %290, 2
  %313 = getelementptr inbounds %struct.GF*, %struct.GF** %260, i64 %312
  %314 = load %struct.GF*, %struct.GF** %313, align 8, !tbaa !9
  %315 = add nsw i64 %312, %255
  %316 = getelementptr inbounds i8, i8* %0, i64 %315
  %317 = load i8, i8* %316, align 1, !tbaa !3
  %318 = zext i8 %317 to i64
  %319 = getelementptr inbounds %struct.GF, %struct.GF* %314, i64 %318, i32 0
  %320 = load i8, i8* %319, align 1, !tbaa.struct !19
  %321 = xor i8 %320, %311
  %322 = or i64 %290, 3
  %323 = getelementptr inbounds %struct.GF*, %struct.GF** %260, i64 %322
  %324 = load %struct.GF*, %struct.GF** %323, align 8, !tbaa !9
  %325 = add nsw i64 %322, %255
  %326 = getelementptr inbounds i8, i8* %0, i64 %325
  %327 = load i8, i8* %326, align 1, !tbaa !3
  %328 = zext i8 %327 to i64
  %329 = getelementptr inbounds %struct.GF, %struct.GF* %324, i64 %328, i32 0
  %330 = load i8, i8* %329, align 1, !tbaa.struct !19
  %331 = xor i8 %330, %321
  %332 = add nuw nsw i64 %290, 4
  %333 = add i64 %292, -4
  %334 = icmp eq i64 %333, 0
  br i1 %334, label %262, label %289

335:                                              ; preds = %354, %288
  %336 = phi i64 [ 0, %288 ], [ %388, %354 ]
  br i1 %253, label %350, label %337

337:                                              ; preds = %335, %337
  %338 = phi i64 [ %347, %337 ], [ %336, %335 ]
  %339 = phi i64 [ %348, %337 ], [ %250, %335 ]
  %340 = getelementptr inbounds %struct.GF, %struct.GF* %189, i64 %338, i32 0
  %341 = load i8, i8* %340, align 1, !tbaa !35
  %342 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %338, i64 0
  %343 = load i32, i32* %342, align 4, !tbaa !22
  %344 = sext i32 %343 to i64
  %345 = add nsw i64 %255, %344
  %346 = getelementptr inbounds i8, i8* %0, i64 %345
  store i8 %341, i8* %346, align 1, !tbaa !3
  %347 = add nuw nsw i64 %338, 1
  %348 = add i64 %339, -1
  %349 = icmp eq i64 %348, 0
  br i1 %349, label %350, label %337, !llvm.loop !38

350:                                              ; preds = %337, %335
  %351 = add nuw nsw i32 %256, 1
  %352 = add nsw i64 %255, %242
  %353 = icmp eq i32 %351, %14
  br i1 %353, label %257, label %254

354:                                              ; preds = %288, %354
  %355 = phi i64 [ %388, %354 ], [ 0, %288 ]
  %356 = phi i64 [ %389, %354 ], [ %252, %288 ]
  %357 = getelementptr inbounds %struct.GF, %struct.GF* %189, i64 %355, i32 0
  %358 = load i8, i8* %357, align 4, !tbaa !35
  %359 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %355, i64 0
  %360 = load i32, i32* %359, align 4, !tbaa !22
  %361 = sext i32 %360 to i64
  %362 = add nsw i64 %255, %361
  %363 = getelementptr inbounds i8, i8* %0, i64 %362
  store i8 %358, i8* %363, align 1, !tbaa !3
  %364 = or i64 %355, 1
  %365 = getelementptr inbounds %struct.GF, %struct.GF* %189, i64 %364, i32 0
  %366 = load i8, i8* %365, align 1, !tbaa !35
  %367 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %364, i64 0
  %368 = load i32, i32* %367, align 4, !tbaa !22
  %369 = sext i32 %368 to i64
  %370 = add nsw i64 %255, %369
  %371 = getelementptr inbounds i8, i8* %0, i64 %370
  store i8 %366, i8* %371, align 1, !tbaa !3
  %372 = or i64 %355, 2
  %373 = getelementptr inbounds %struct.GF, %struct.GF* %189, i64 %372, i32 0
  %374 = load i8, i8* %373, align 2, !tbaa !35
  %375 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %372, i64 0
  %376 = load i32, i32* %375, align 4, !tbaa !22
  %377 = sext i32 %376 to i64
  %378 = add nsw i64 %255, %377
  %379 = getelementptr inbounds i8, i8* %0, i64 %378
  store i8 %374, i8* %379, align 1, !tbaa !3
  %380 = or i64 %355, 3
  %381 = getelementptr inbounds %struct.GF, %struct.GF* %189, i64 %380, i32 0
  %382 = load i8, i8* %381, align 1, !tbaa !35
  %383 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %380, i64 0
  %384 = load i32, i32* %383, align 4, !tbaa !22
  %385 = sext i32 %384 to i64
  %386 = add nsw i64 %255, %385
  %387 = getelementptr inbounds i8, i8* %0, i64 %386
  store i8 %382, i8* %387, align 1, !tbaa !3
  %388 = add nuw nsw i64 %355, 4
  %389 = add i64 %356, -4
  %390 = icmp eq i64 %389, 0
  br i1 %390, label %335, label %354

391:                                              ; preds = %20, %27, %77, %55, %12, %9, %5, %257
  %392 = phi i32 [ 0, %257 ], [ 1, %5 ], [ 2, %9 ], [ 3, %12 ], [ 4, %77 ], [ 4, %55 ], [ 4, %27 ], [ 4, %20 ]
  ret i32 %392
}

; Function Attrs: nounwind uwtable
define internal fastcc void @"?mat_inverse@@YAXPEAUGF@@H@Z"(%struct.GF* nocapture %0, i32 %1) unnamed_addr #0 {
  %3 = mul nsw i32 %1, %1
  %4 = zext i32 %3 to i64
  %5 = alloca %struct.GF, i64 %4, align 16
  %6 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 0, i32 0
  call void @llvm.memset.p0i8.i64(i8* nonnull align 16 %6, i8 0, i64 %4, i1 false)
  %7 = icmp sgt i32 %1, 0
  br i1 %7, label %8, label %330

8:                                                ; preds = %2
  %9 = zext i32 %1 to i64
  %10 = zext i32 %1 to i64
  %11 = add nsw i64 %10, -1
  %12 = and i64 %10, 3
  %13 = icmp ult i64 %11, 3
  br i1 %13, label %16, label %14

14:                                               ; preds = %8
  %15 = and i64 %10, 4294967292
  br label %37

16:                                               ; preds = %37, %8
  %17 = phi i64 [ 0, %8 ], [ %55, %37 ]
  %18 = icmp eq i64 %12, 0
  br i1 %18, label %28, label %19

19:                                               ; preds = %16, %19
  %20 = phi i64 [ %25, %19 ], [ %17, %16 ]
  %21 = phi i64 [ %26, %19 ], [ %12, %16 ]
  %22 = mul nsw i64 %20, %9
  %23 = add nuw nsw i64 %22, %20
  %24 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %23, i32 0
  store i8 1, i8* %24, align 1, !tbaa !3
  %25 = add nuw nsw i64 %20, 1
  %26 = add i64 %21, -1
  %27 = icmp eq i64 %26, 0
  br i1 %27, label %28, label %19, !llvm.loop !39

28:                                               ; preds = %19, %16
  br i1 %7, label %29, label %330

29:                                               ; preds = %28
  %30 = zext i32 %1 to i64
  %31 = zext i32 %1 to i64
  %32 = add nsw i64 %31, -2
  %33 = and i64 %31, 1
  %34 = icmp eq i32 %1, 1
  %35 = and i64 %31, 4294967294
  %36 = icmp eq i64 %33, 0
  br label %71

37:                                               ; preds = %37, %14
  %38 = phi i64 [ 0, %14 ], [ %55, %37 ]
  %39 = phi i64 [ %15, %14 ], [ %56, %37 ]
  %40 = mul nsw i64 %38, %9
  %41 = add nuw nsw i64 %40, %38
  %42 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %41, i32 0
  store i8 1, i8* %42, align 4, !tbaa !3
  %43 = or i64 %38, 1
  %44 = mul nsw i64 %43, %9
  %45 = add nuw nsw i64 %44, %43
  %46 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %45, i32 0
  store i8 1, i8* %46, align 1, !tbaa !3
  %47 = or i64 %38, 2
  %48 = mul nsw i64 %47, %9
  %49 = add nuw nsw i64 %48, %47
  %50 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %49, i32 0
  store i8 1, i8* %50, align 2, !tbaa !3
  %51 = or i64 %38, 3
  %52 = mul nsw i64 %51, %9
  %53 = add nuw nsw i64 %52, %51
  %54 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %53, i32 0
  store i8 1, i8* %54, align 1, !tbaa !3
  %55 = add nuw nsw i64 %38, 4
  %56 = add i64 %39, -4
  %57 = icmp eq i64 %56, 0
  br i1 %57, label %16, label %37

58:                                               ; preds = %201, %71
  %59 = add nuw nsw i64 %73, 1
  %60 = icmp eq i64 %76, %31
  br i1 %60, label %61, label %71

61:                                               ; preds = %58
  %62 = add i32 %1, -1
  br i1 %7, label %63, label %330

63:                                               ; preds = %61
  %64 = add nsw i32 %1, -2
  %65 = zext i32 %62 to i64
  %66 = and i64 %65, 1
  %67 = icmp eq i64 %66, 0
  %68 = add i32 %1, -2
  %69 = add nsw i64 %65, -1
  %70 = icmp eq i32 %62, 0
  br label %215

71:                                               ; preds = %58, %29
  %72 = phi i64 [ 0, %29 ], [ %76, %58 ]
  %73 = phi i64 [ 1, %29 ], [ %59, %58 ]
  %74 = xor i64 %72, 1
  %75 = add nuw i64 %74, %31
  %76 = add nuw nsw i64 %72, 1
  %77 = icmp ult i64 %76, %30
  br i1 %77, label %78, label %58

78:                                               ; preds = %71
  %79 = mul nsw i64 %72, %30
  %80 = add nuw nsw i64 %79, %72
  %81 = getelementptr inbounds %struct.GF, %struct.GF* %0, i64 %80, i32 0
  %82 = and i64 %75, 1
  %83 = icmp eq i64 %82, 0
  %84 = add nuw nsw i64 %73, %79
  %85 = getelementptr inbounds %struct.GF, %struct.GF* %0, i64 %84, i32 0
  %86 = add nuw nsw i64 %73, 1
  %87 = icmp eq i64 %32, %72
  br label %88

88:                                               ; preds = %201, %78
  %89 = phi i64 [ %73, %78 ], [ %202, %201 ]
  %90 = load i8, i8* %81, align 1, !tbaa.struct !19
  %91 = mul nsw i64 %89, %30
  %92 = add nuw nsw i64 %91, %72
  %93 = getelementptr inbounds %struct.GF, %struct.GF* %0, i64 %92, i32 0
  %94 = load i8, i8* %93, align 1, !tbaa.struct !19
  %95 = icmp ult i8 %90, 2
  br i1 %95, label %104, label %96

96:                                               ; preds = %88
  %97 = zext i8 %90 to i64
  %98 = getelementptr inbounds [256 x i8], [256 x i8]* @"?exp_table@GF@@0QBEB", i64 0, i64 %97
  %99 = load i8, i8* %98, align 1, !tbaa !3, !noalias !40
  %100 = zext i8 %99 to i64
  %101 = sub nuw nsw i64 256, %100
  %102 = getelementptr inbounds [256 x i8], [256 x i8]* @"?value_table@GF@@0QBEB", i64 0, i64 %101
  %103 = load i8, i8* %102, align 1, !tbaa !3, !noalias !40
  br label %104

104:                                              ; preds = %88, %96
  %105 = phi i8 [ %103, %96 ], [ %90, %88 ]
  %106 = zext i8 %105 to i64
  %107 = shl nuw nsw i64 %106, 8
  %108 = zext i8 %94 to i64
  %109 = or i64 %107, %108
  %110 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %109, i32 0
  %111 = load i8, i8* %110, align 1, !tbaa !3, !noalias !45
  %112 = icmp eq i8 %111, 0
  br i1 %112, label %201, label %113

113:                                              ; preds = %104
  store i8 0, i8* %93, align 1, !tbaa !3
  %114 = zext i8 %111 to i64
  %115 = shl nuw nsw i64 %114, 8
  br i1 %83, label %126, label %116

116:                                              ; preds = %113
  %117 = load i8, i8* %85, align 1, !tbaa.struct !19
  %118 = zext i8 %117 to i64
  %119 = or i64 %115, %118
  %120 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %119, i32 0
  %121 = load i8, i8* %120, align 1, !tbaa !3, !noalias !48
  %122 = add nuw nsw i64 %73, %91
  %123 = getelementptr inbounds %struct.GF, %struct.GF* %0, i64 %122, i32 0
  %124 = load i8, i8* %123, align 1, !tbaa !35
  %125 = xor i8 %124, %121
  store i8 %125, i8* %123, align 1, !tbaa !35
  br label %126

126:                                              ; preds = %116, %113
  %127 = phi i64 [ %86, %116 ], [ %73, %113 ]
  br i1 %87, label %128, label %131

128:                                              ; preds = %131, %126
  %129 = zext i8 %111 to i64
  %130 = shl nuw nsw i64 %129, 8
  br i1 %34, label %187, label %158

131:                                              ; preds = %126, %131
  %132 = phi i64 [ %156, %131 ], [ %127, %126 ]
  %133 = add nuw nsw i64 %132, %79
  %134 = getelementptr inbounds %struct.GF, %struct.GF* %0, i64 %133, i32 0
  %135 = load i8, i8* %134, align 1, !tbaa.struct !19
  %136 = zext i8 %135 to i64
  %137 = or i64 %115, %136
  %138 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %137, i32 0
  %139 = load i8, i8* %138, align 1, !tbaa !3, !noalias !48
  %140 = add nuw nsw i64 %132, %91
  %141 = getelementptr inbounds %struct.GF, %struct.GF* %0, i64 %140, i32 0
  %142 = load i8, i8* %141, align 1, !tbaa !35
  %143 = xor i8 %142, %139
  store i8 %143, i8* %141, align 1, !tbaa !35
  %144 = add nuw nsw i64 %132, 1
  %145 = add nuw nsw i64 %144, %79
  %146 = getelementptr inbounds %struct.GF, %struct.GF* %0, i64 %145, i32 0
  %147 = load i8, i8* %146, align 1, !tbaa.struct !19
  %148 = zext i8 %147 to i64
  %149 = or i64 %115, %148
  %150 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %149, i32 0
  %151 = load i8, i8* %150, align 1, !tbaa !3, !noalias !48
  %152 = add nuw nsw i64 %144, %91
  %153 = getelementptr inbounds %struct.GF, %struct.GF* %0, i64 %152, i32 0
  %154 = load i8, i8* %153, align 1, !tbaa !35
  %155 = xor i8 %154, %151
  store i8 %155, i8* %153, align 1, !tbaa !35
  %156 = add nuw nsw i64 %132, 2
  %157 = icmp eq i64 %156, %31
  br i1 %157, label %128, label %131

158:                                              ; preds = %128, %158
  %159 = phi i64 [ %184, %158 ], [ 0, %128 ]
  %160 = phi i64 [ %185, %158 ], [ %35, %128 ]
  %161 = add nuw nsw i64 %159, %79
  %162 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %161, i32 0
  %163 = load i8, i8* %162, align 1, !tbaa.struct !19
  %164 = zext i8 %163 to i64
  %165 = or i64 %130, %164
  %166 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %165, i32 0
  %167 = load i8, i8* %166, align 1, !tbaa !3, !noalias !51
  %168 = add nuw nsw i64 %159, %91
  %169 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %168, i32 0
  %170 = load i8, i8* %169, align 1, !tbaa !35
  %171 = xor i8 %170, %167
  store i8 %171, i8* %169, align 1, !tbaa !35
  %172 = or i64 %159, 1
  %173 = add nuw nsw i64 %172, %79
  %174 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %173, i32 0
  %175 = load i8, i8* %174, align 1, !tbaa.struct !19
  %176 = zext i8 %175 to i64
  %177 = or i64 %130, %176
  %178 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %177, i32 0
  %179 = load i8, i8* %178, align 1, !tbaa !3, !noalias !51
  %180 = add nuw nsw i64 %172, %91
  %181 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %180, i32 0
  %182 = load i8, i8* %181, align 1, !tbaa !35
  %183 = xor i8 %182, %179
  store i8 %183, i8* %181, align 1, !tbaa !35
  %184 = add nuw nsw i64 %159, 2
  %185 = add i64 %160, -2
  %186 = icmp eq i64 %185, 0
  br i1 %186, label %187, label %158

187:                                              ; preds = %158, %128
  %188 = phi i64 [ 0, %128 ], [ %184, %158 ]
  br i1 %36, label %201, label %189

189:                                              ; preds = %187
  %190 = add nuw nsw i64 %188, %79
  %191 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %190, i32 0
  %192 = load i8, i8* %191, align 1, !tbaa.struct !19
  %193 = zext i8 %192 to i64
  %194 = or i64 %130, %193
  %195 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %194, i32 0
  %196 = load i8, i8* %195, align 1, !tbaa !3, !noalias !51
  %197 = add nuw nsw i64 %188, %91
  %198 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %197, i32 0
  %199 = load i8, i8* %198, align 1, !tbaa !35
  %200 = xor i8 %199, %196
  store i8 %200, i8* %198, align 1, !tbaa !35
  br label %201

201:                                              ; preds = %189, %187, %104
  %202 = add nuw nsw i64 %89, 1
  %203 = icmp eq i64 %202, %31
  br i1 %203, label %58, label %88

204:                                              ; preds = %327
  %205 = add i32 %217, -1
  %206 = add nsw i64 %216, -1
  br i1 %219, label %215, label %207

207:                                              ; preds = %215, %204
  br i1 %7, label %208, label %330

208:                                              ; preds = %207
  %209 = zext i32 %1 to i64
  %210 = zext i32 %1 to i64
  %211 = and i64 %210, 1
  %212 = icmp eq i32 %1, 1
  %213 = and i64 %210, 4294967294
  %214 = icmp eq i64 %211, 0
  br label %333

215:                                              ; preds = %63, %204
  %216 = phi i64 [ %65, %63 ], [ %206, %204 ]
  %217 = phi i32 [ %64, %63 ], [ %205, %204 ]
  %218 = zext i32 %217 to i64
  %219 = icmp sgt i64 %216, 0
  br i1 %219, label %220, label %207

220:                                              ; preds = %215
  %221 = trunc i64 %216 to i32
  %222 = mul nsw i32 %221, %1
  %223 = add nsw i32 %222, %221
  %224 = zext i32 %223 to i64
  %225 = getelementptr inbounds %struct.GF, %struct.GF* %0, i64 %224, i32 0
  %226 = add nsw i32 %62, %222
  %227 = zext i32 %226 to i64
  %228 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %227, i32 0
  br label %229

229:                                              ; preds = %220, %327
  %230 = phi i64 [ %218, %220 ], [ %329, %327 ]
  %231 = load i8, i8* %225, align 1, !tbaa.struct !19
  %232 = trunc i64 %230 to i32
  %233 = mul nsw i32 %232, %1
  %234 = add nsw i32 %233, %221
  %235 = zext i32 %234 to i64
  %236 = getelementptr inbounds %struct.GF, %struct.GF* %0, i64 %235, i32 0
  %237 = load i8, i8* %236, align 1, !tbaa.struct !19
  %238 = icmp ult i8 %231, 2
  br i1 %238, label %247, label %239

239:                                              ; preds = %229
  %240 = zext i8 %231 to i64
  %241 = getelementptr inbounds [256 x i8], [256 x i8]* @"?exp_table@GF@@0QBEB", i64 0, i64 %240
  %242 = load i8, i8* %241, align 1, !tbaa !3, !noalias !54
  %243 = zext i8 %242 to i64
  %244 = sub nuw nsw i64 256, %243
  %245 = getelementptr inbounds [256 x i8], [256 x i8]* @"?value_table@GF@@0QBEB", i64 0, i64 %244
  %246 = load i8, i8* %245, align 1, !tbaa !3, !noalias !54
  br label %247

247:                                              ; preds = %229, %239
  %248 = phi i8 [ %246, %239 ], [ %231, %229 ]
  %249 = zext i8 %248 to i64
  %250 = shl nuw nsw i64 %249, 8
  %251 = zext i8 %237 to i64
  %252 = or i64 %250, %251
  %253 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %252, i32 0
  %254 = load i8, i8* %253, align 1, !tbaa !3, !noalias !59
  %255 = icmp eq i8 %254, 0
  br i1 %255, label %327, label %256

256:                                              ; preds = %247
  %257 = zext i8 %254 to i64
  %258 = shl nuw nsw i64 %257, 8
  br label %276

259:                                              ; preds = %276
  %260 = zext i8 %254 to i64
  %261 = shl nuw nsw i64 %260, 8
  br i1 %67, label %262, label %273

262:                                              ; preds = %259
  %263 = load i8, i8* %228, align 1, !tbaa.struct !19
  %264 = zext i8 %263 to i64
  %265 = or i64 %261, %264
  %266 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %265, i32 0
  %267 = load i8, i8* %266, align 1, !tbaa !3, !noalias !62
  %268 = add nsw i32 %62, %233
  %269 = zext i32 %268 to i64
  %270 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %269, i32 0
  %271 = load i8, i8* %270, align 1, !tbaa !35
  %272 = xor i8 %271, %267
  store i8 %272, i8* %270, align 1, !tbaa !35
  br label %273

273:                                              ; preds = %262, %259
  %274 = phi i64 [ %69, %262 ], [ %65, %259 ]
  %275 = phi i32 [ %68, %262 ], [ %62, %259 ]
  br i1 %70, label %327, label %294

276:                                              ; preds = %256, %276
  %277 = phi i64 [ %218, %256 ], [ %293, %276 ]
  %278 = trunc i64 %277 to i32
  %279 = add nsw i32 %222, %278
  %280 = zext i32 %279 to i64
  %281 = getelementptr inbounds %struct.GF, %struct.GF* %0, i64 %280, i32 0
  %282 = load i8, i8* %281, align 1, !tbaa.struct !19
  %283 = zext i8 %282 to i64
  %284 = or i64 %258, %283
  %285 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %284, i32 0
  %286 = load i8, i8* %285, align 1, !tbaa !3, !noalias !65
  %287 = add nsw i32 %233, %278
  %288 = zext i32 %287 to i64
  %289 = getelementptr inbounds %struct.GF, %struct.GF* %0, i64 %288, i32 0
  %290 = load i8, i8* %289, align 1, !tbaa !35
  %291 = xor i8 %290, %286
  store i8 %291, i8* %289, align 1, !tbaa !35
  %292 = icmp sgt i32 %278, 0
  %293 = add nsw i64 %277, -1
  br i1 %292, label %276, label %259

294:                                              ; preds = %273, %294
  %295 = phi i64 [ %326, %294 ], [ %274, %273 ]
  %296 = phi i32 [ %324, %294 ], [ %275, %273 ]
  %297 = add nsw i32 %296, %222
  %298 = zext i32 %297 to i64
  %299 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %298, i32 0
  %300 = load i8, i8* %299, align 1, !tbaa.struct !19
  %301 = zext i8 %300 to i64
  %302 = or i64 %261, %301
  %303 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %302, i32 0
  %304 = load i8, i8* %303, align 1, !tbaa !3, !noalias !62
  %305 = add nsw i32 %296, %233
  %306 = zext i32 %305 to i64
  %307 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %306, i32 0
  %308 = load i8, i8* %307, align 1, !tbaa !35
  %309 = xor i8 %308, %304
  store i8 %309, i8* %307, align 1, !tbaa !35
  %310 = add nsw i32 %296, -1
  %311 = add nsw i32 %310, %222
  %312 = zext i32 %311 to i64
  %313 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %312, i32 0
  %314 = load i8, i8* %313, align 1, !tbaa.struct !19
  %315 = zext i8 %314 to i64
  %316 = or i64 %261, %315
  %317 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %316, i32 0
  %318 = load i8, i8* %317, align 1, !tbaa !3, !noalias !62
  %319 = add nsw i32 %310, %233
  %320 = zext i32 %319 to i64
  %321 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %320, i32 0
  %322 = load i8, i8* %321, align 1, !tbaa !35
  %323 = xor i8 %322, %318
  store i8 %323, i8* %321, align 1, !tbaa !35
  %324 = add nsw i32 %296, -2
  %325 = icmp sgt i64 %295, 1
  %326 = add nsw i64 %295, -2
  br i1 %325, label %294, label %327

327:                                              ; preds = %273, %294, %247
  %328 = icmp sgt i32 %232, 0
  %329 = add nsw i64 %230, -1
  br i1 %328, label %229, label %204

330:                                              ; preds = %385, %2, %28, %61, %207
  %331 = getelementptr %struct.GF, %struct.GF* %0, i64 0, i32 0
  %332 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 0, i32 0
  call void @llvm.memcpy.p0i8.p0i8.i64(i8* align 1 %331, i8* nonnull align 16 %332, i64 %4, i1 false)
  ret void

333:                                              ; preds = %385, %208
  %334 = phi i64 [ 0, %208 ], [ %386, %385 ]
  %335 = mul nsw i64 %334, %209
  %336 = add nuw nsw i64 %335, %334
  %337 = getelementptr inbounds %struct.GF, %struct.GF* %0, i64 %336, i32 0
  %338 = load i8, i8* %337, align 1, !tbaa !35, !noalias !68
  %339 = icmp ult i8 %338, 2
  br i1 %339, label %348, label %340

340:                                              ; preds = %333
  %341 = zext i8 %338 to i64
  %342 = getelementptr inbounds [256 x i8], [256 x i8]* @"?exp_table@GF@@0QBEB", i64 0, i64 %341
  %343 = load i8, i8* %342, align 1, !tbaa !3, !noalias !68
  %344 = zext i8 %343 to i64
  %345 = sub nuw nsw i64 256, %344
  %346 = getelementptr inbounds [256 x i8], [256 x i8]* @"?value_table@GF@@0QBEB", i64 0, i64 %345
  %347 = load i8, i8* %346, align 1, !tbaa !3, !noalias !68
  br label %348

348:                                              ; preds = %333, %340
  %349 = phi i8 [ %347, %340 ], [ %338, %333 ]
  %350 = icmp eq i8 %349, 1
  br i1 %350, label %385, label %351

351:                                              ; preds = %348
  %352 = zext i8 %349 to i64
  %353 = shl nuw nsw i64 %352, 8
  br i1 %212, label %375, label %354

354:                                              ; preds = %351, %354
  %355 = phi i64 [ %372, %354 ], [ 0, %351 ]
  %356 = phi i64 [ %373, %354 ], [ %213, %351 ]
  %357 = add nuw nsw i64 %355, %335
  %358 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %357, i32 0
  %359 = load i8, i8* %358, align 1, !tbaa.struct !19
  %360 = zext i8 %359 to i64
  %361 = or i64 %353, %360
  %362 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %361, i32 0
  %363 = load i8, i8* %362, align 1, !tbaa !3, !noalias !71
  store i8 %363, i8* %358, align 1, !tbaa !35
  %364 = or i64 %355, 1
  %365 = add nuw nsw i64 %364, %335
  %366 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %365, i32 0
  %367 = load i8, i8* %366, align 1, !tbaa.struct !19
  %368 = zext i8 %367 to i64
  %369 = or i64 %353, %368
  %370 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %369, i32 0
  %371 = load i8, i8* %370, align 1, !tbaa !3, !noalias !71
  store i8 %371, i8* %366, align 1, !tbaa !35
  %372 = add nuw nsw i64 %355, 2
  %373 = add i64 %356, -2
  %374 = icmp eq i64 %373, 0
  br i1 %374, label %375, label %354

375:                                              ; preds = %354, %351
  %376 = phi i64 [ 0, %351 ], [ %372, %354 ]
  br i1 %214, label %385, label %377

377:                                              ; preds = %375
  %378 = add nuw nsw i64 %376, %335
  %379 = getelementptr inbounds %struct.GF, %struct.GF* %5, i64 %378, i32 0
  %380 = load i8, i8* %379, align 1, !tbaa.struct !19
  %381 = zext i8 %380 to i64
  %382 = or i64 %353, %381
  %383 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %382, i32 0
  %384 = load i8, i8* %383, align 1, !tbaa !3, !noalias !71
  store i8 %384, i8* %379, align 1, !tbaa !35
  br label %385

385:                                              ; preds = %377, %375, %348
  %386 = add nuw nsw i64 %334, 1
  %387 = icmp eq i64 %386, %210
  br i1 %387, label %330, label %333
}

; Function Attrs: nounwind uwtable
define dso_local i32 @ec_encode_ssse3(i8* noalias nocapture readonly %0, i32 %1, i8* noalias nocapture %2, i32 %3, i32 %4, i32 %5) local_unnamed_addr #4 {
  %7 = alloca <2 x i64>, align 16
  %8 = alloca <2 x i64>, align 16
  %9 = add i32 %4, -1
  %10 = icmp ugt i32 %9, 253
  br i1 %10, label %569, label %11

11:                                               ; preds = %6
  %12 = add i32 %5, -1
  %13 = icmp ugt i32 %12, 253
  br i1 %13, label %569, label %14

14:                                               ; preds = %11
  %15 = srem i32 %1, %4
  %16 = sdiv i32 %1, %4
  %17 = icmp eq i32 %15, 0
  br i1 %17, label %18, label %569

18:                                               ; preds = %14
  %19 = srem i32 %3, %5
  %20 = sdiv i32 %3, %5
  %21 = icmp eq i32 %19, 0
  br i1 %21, label %22, label %569

22:                                               ; preds = %18
  %23 = icmp eq i32 %16, %20
  br i1 %23, label %24, label %569

24:                                               ; preds = %22
  %25 = mul nuw nsw i32 %5, %4
  %26 = zext i32 %25 to i64
  %27 = tail call i8* @llvm.stacksave()
  %28 = alloca [2 x <2 x i64>], i64 %26, align 16
  br label %49

29:                                               ; preds = %61
  %30 = zext i32 %4 to i64
  %31 = alloca <2 x i64>, i64 %30, align 16
  %32 = add i32 %16, -16
  %33 = icmp slt i32 %16, 16
  br i1 %33, label %345, label %34

34:                                               ; preds = %29
  %35 = icmp sgt i32 %4, 0
  %36 = bitcast <2 x i64>* %7 to i8*
  %37 = sext i32 %5 to i64
  %38 = sext i32 %4 to i64
  %39 = shl i32 %5, 4
  %40 = sext i32 %39 to i64
  %41 = zext i32 %5 to i64
  %42 = getelementptr inbounds i8, i8* %36, i64 12
  %43 = getelementptr inbounds i8, i8* %36, i64 13
  %44 = getelementptr inbounds i8, i8* %36, i64 14
  %45 = getelementptr inbounds i8, i8* %36, i64 15
  %46 = icmp ult i32 %4, 16
  %47 = and i64 %30, 4294967280
  %48 = icmp eq i64 %47, %30
  br label %113

49:                                               ; preds = %61, %24
  %50 = phi i32 [ 0, %24 ], [ %62, %61 ]
  %51 = phi i64 [ 0, %24 ], [ %86, %61 ]
  %52 = urem i32 %50, 255
  %53 = add nuw nsw i32 %52, 1
  %54 = zext i32 %53 to i64
  %55 = getelementptr inbounds [256 x i8], [256 x i8]* @"?value_table@GF@@0QBEB", i64 0, i64 %54
  %56 = load i8, i8* %55, align 1, !tbaa !3, !noalias !74
  %57 = zext i8 %56 to i64
  %58 = getelementptr inbounds [256 x i8], [256 x i8]* @"?exp_table@GF@@0QBEB", i64 0, i64 %57
  %59 = shl i64 %51, 32
  %60 = ashr exact i64 %59, 32
  br label %64

61:                                               ; preds = %84
  %62 = add nuw nsw i32 %50, 1
  %63 = icmp eq i32 %62, %5
  br i1 %63, label %29, label %49

64:                                               ; preds = %84, %49
  %65 = phi i64 [ %60, %49 ], [ %86, %84 ]
  %66 = phi i32 [ 0, %49 ], [ %85, %84 ]
  %67 = icmp eq i32 %66, 0
  br i1 %67, label %77, label %68

68:                                               ; preds = %64
  %69 = load i8, i8* %58, align 1, !tbaa !3, !noalias !77
  %70 = zext i8 %69 to i32
  %71 = mul nsw i32 %66, %70
  %72 = urem i32 %71, 255
  %73 = add nuw nsw i32 %72, 1
  %74 = zext i32 %73 to i64
  %75 = getelementptr inbounds [256 x i8], [256 x i8]* @"?value_table@GF@@0QBEB", i64 0, i64 %74
  %76 = load i8, i8* %75, align 1, !tbaa !3, !noalias !80
  br label %77

77:                                               ; preds = %64, %68
  %78 = phi i8 [ %76, %68 ], [ 1, %64 ]
  %79 = zext i8 %78 to i64
  %80 = getelementptr inbounds [2 x <2 x i64>], [2 x <2 x i64>]* %28, i64 %65
  %81 = bitcast [2 x <2 x i64>]* %80 to i8*
  %82 = getelementptr inbounds [2 x <2 x i64>], [2 x <2 x i64>]* %28, i64 %65, i64 1
  %83 = bitcast <2 x i64>* %82 to i8*
  br label %88

84:                                               ; preds = %88
  %85 = add nuw nsw i32 %66, 1
  %86 = add nsw i64 %65, 1
  %87 = icmp eq i32 %85, %4
  br i1 %87, label %61, label %64

88:                                               ; preds = %88, %77
  %89 = phi i64 [ 0, %77 ], [ %111, %88 ]
  %90 = shl i64 %89, 8
  %91 = or i64 %90, %79
  %92 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %91, i32 0
  %93 = load i8, i8* %92, align 1, !tbaa !3, !noalias !83
  %94 = getelementptr inbounds i8, i8* %81, i64 %89
  store i8 %93, i8* %94, align 2, !tbaa !3
  %95 = shl nuw nsw i64 %89, 12
  %96 = or i64 %95, %79
  %97 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %96, i32 0
  %98 = load i8, i8* %97, align 1, !tbaa !3, !noalias !86
  %99 = getelementptr inbounds i8, i8* %83, i64 %89
  store i8 %98, i8* %99, align 2, !tbaa !3
  %100 = or i64 %89, 1
  %101 = shl i64 %100, 8
  %102 = or i64 %101, %79
  %103 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %102, i32 0
  %104 = load i8, i8* %103, align 1, !tbaa !3, !noalias !83
  %105 = getelementptr inbounds i8, i8* %81, i64 %100
  store i8 %104, i8* %105, align 1, !tbaa !3
  %106 = shl nuw nsw i64 %100, 12
  %107 = or i64 %106, %79
  %108 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %107, i32 0
  %109 = load i8, i8* %108, align 1, !tbaa !3, !noalias !86
  %110 = getelementptr inbounds i8, i8* %83, i64 %100
  store i8 %109, i8* %110, align 1, !tbaa !3
  %111 = add nuw nsw i64 %89, 2
  %112 = icmp eq i64 %111, 16
  br i1 %112, label %84, label %88

113:                                              ; preds = %34, %250
  %114 = phi i64 [ 0, %34 ], [ %252, %250 ]
  %115 = phi i32 [ 0, %34 ], [ %236, %250 ]
  %116 = phi i32 [ 0, %34 ], [ %251, %250 ]
  br label %134

117:                                              ; preds = %235
  %118 = getelementptr inbounds i8, i8* %2, i64 %114
  %119 = getelementptr inbounds i8, i8* %118, i64 %37
  %120 = getelementptr inbounds i8, i8* %119, i64 %37
  %121 = getelementptr inbounds i8, i8* %120, i64 %37
  %122 = getelementptr inbounds i8, i8* %121, i64 %37
  %123 = getelementptr inbounds i8, i8* %122, i64 %37
  %124 = getelementptr inbounds i8, i8* %123, i64 %37
  %125 = getelementptr inbounds i8, i8* %124, i64 %37
  %126 = getelementptr inbounds i8, i8* %125, i64 %37
  %127 = getelementptr inbounds i8, i8* %126, i64 %37
  %128 = getelementptr inbounds i8, i8* %127, i64 %37
  %129 = getelementptr inbounds i8, i8* %128, i64 %37
  %130 = getelementptr inbounds i8, i8* %129, i64 %37
  %131 = getelementptr inbounds i8, i8* %130, i64 %37
  %132 = getelementptr inbounds i8, i8* %131, i64 %37
  %133 = getelementptr inbounds i8, i8* %132, i64 %37
  br label %254

134:                                              ; preds = %235, %113
  %135 = phi i64 [ 0, %113 ], [ %237, %235 ]
  %136 = phi i32 [ %115, %113 ], [ %236, %235 ]
  br i1 %35, label %137, label %235

137:                                              ; preds = %134
  %138 = sext i32 %136 to i64
  br i1 %46, label %139, label %142

139:                                              ; preds = %231, %137
  %140 = phi i64 [ %138, %137 ], [ %143, %231 ]
  %141 = phi i64 [ 0, %137 ], [ %47, %231 ]
  br label %239

142:                                              ; preds = %137
  %143 = add nsw i64 %47, %138
  br label %144

144:                                              ; preds = %144, %142
  %145 = phi i64 [ 0, %142 ], [ %229, %144 ]
  %146 = add i64 %145, %138
  %147 = or i64 %145, 1
  %148 = or i64 %145, 2
  %149 = or i64 %145, 3
  %150 = or i64 %145, 4
  %151 = or i64 %145, 5
  %152 = or i64 %145, 6
  %153 = or i64 %145, 7
  %154 = or i64 %145, 8
  %155 = or i64 %145, 9
  %156 = or i64 %145, 10
  %157 = or i64 %145, 11
  %158 = or i64 %145, 12
  %159 = or i64 %145, 13
  %160 = or i64 %145, 14
  %161 = or i64 %145, 15
  %162 = getelementptr inbounds i8, i8* %0, i64 %146
  %163 = bitcast i8* %162 to <16 x i8>*
  %164 = load <16 x i8>, <16 x i8>* %163, align 1, !tbaa !3
  %165 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %145
  %166 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %147
  %167 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %148
  %168 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %149
  %169 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %150
  %170 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %151
  %171 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %152
  %172 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %153
  %173 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %154
  %174 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %155
  %175 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %156
  %176 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %157
  %177 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %158
  %178 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %159
  %179 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %160
  %180 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %161
  %181 = bitcast <2 x i64>* %165 to i8*
  %182 = bitcast <2 x i64>* %166 to i8*
  %183 = bitcast <2 x i64>* %167 to i8*
  %184 = bitcast <2 x i64>* %168 to i8*
  %185 = bitcast <2 x i64>* %169 to i8*
  %186 = bitcast <2 x i64>* %170 to i8*
  %187 = bitcast <2 x i64>* %171 to i8*
  %188 = bitcast <2 x i64>* %172 to i8*
  %189 = bitcast <2 x i64>* %173 to i8*
  %190 = bitcast <2 x i64>* %174 to i8*
  %191 = bitcast <2 x i64>* %175 to i8*
  %192 = bitcast <2 x i64>* %176 to i8*
  %193 = bitcast <2 x i64>* %177 to i8*
  %194 = bitcast <2 x i64>* %178 to i8*
  %195 = bitcast <2 x i64>* %179 to i8*
  %196 = bitcast <2 x i64>* %180 to i8*
  %197 = getelementptr inbounds i8, i8* %181, i64 %135
  %198 = getelementptr inbounds i8, i8* %182, i64 %135
  %199 = getelementptr inbounds i8, i8* %183, i64 %135
  %200 = getelementptr inbounds i8, i8* %184, i64 %135
  %201 = getelementptr inbounds i8, i8* %185, i64 %135
  %202 = getelementptr inbounds i8, i8* %186, i64 %135
  %203 = getelementptr inbounds i8, i8* %187, i64 %135
  %204 = getelementptr inbounds i8, i8* %188, i64 %135
  %205 = getelementptr inbounds i8, i8* %189, i64 %135
  %206 = getelementptr inbounds i8, i8* %190, i64 %135
  %207 = getelementptr inbounds i8, i8* %191, i64 %135
  %208 = getelementptr inbounds i8, i8* %192, i64 %135
  %209 = getelementptr inbounds i8, i8* %193, i64 %135
  %210 = getelementptr inbounds i8, i8* %194, i64 %135
  %211 = getelementptr inbounds i8, i8* %195, i64 %135
  %212 = getelementptr inbounds i8, i8* %196, i64 %135
  %213 = extractelement <16 x i8> %164, i32 0
  store i8 %213, i8* %197, align 1, !tbaa !3
  %214 = extractelement <16 x i8> %164, i32 1
  store i8 %214, i8* %198, align 1, !tbaa !3
  %215 = extractelement <16 x i8> %164, i32 2
  store i8 %215, i8* %199, align 1, !tbaa !3
  %216 = extractelement <16 x i8> %164, i32 3
  store i8 %216, i8* %200, align 1, !tbaa !3
  %217 = extractelement <16 x i8> %164, i32 4
  store i8 %217, i8* %201, align 1, !tbaa !3
  %218 = extractelement <16 x i8> %164, i32 5
  store i8 %218, i8* %202, align 1, !tbaa !3
  %219 = extractelement <16 x i8> %164, i32 6
  store i8 %219, i8* %203, align 1, !tbaa !3
  %220 = extractelement <16 x i8> %164, i32 7
  store i8 %220, i8* %204, align 1, !tbaa !3
  %221 = extractelement <16 x i8> %164, i32 8
  store i8 %221, i8* %205, align 1, !tbaa !3
  %222 = extractelement <16 x i8> %164, i32 9
  store i8 %222, i8* %206, align 1, !tbaa !3
  %223 = extractelement <16 x i8> %164, i32 10
  store i8 %223, i8* %207, align 1, !tbaa !3
  %224 = extractelement <16 x i8> %164, i32 11
  store i8 %224, i8* %208, align 1, !tbaa !3
  %225 = extractelement <16 x i8> %164, i32 12
  store i8 %225, i8* %209, align 1, !tbaa !3
  %226 = extractelement <16 x i8> %164, i32 13
  store i8 %226, i8* %210, align 1, !tbaa !3
  %227 = extractelement <16 x i8> %164, i32 14
  store i8 %227, i8* %211, align 1, !tbaa !3
  %228 = extractelement <16 x i8> %164, i32 15
  store i8 %228, i8* %212, align 1, !tbaa !3
  %229 = add i64 %145, 16
  %230 = icmp eq i64 %229, %47
  br i1 %230, label %231, label %144, !llvm.loop !89

231:                                              ; preds = %144
  br i1 %48, label %232, label %139

232:                                              ; preds = %239, %231
  %233 = phi i64 [ %143, %231 ], [ %242, %239 ]
  %234 = trunc i64 %233 to i32
  br label %235

235:                                              ; preds = %232, %134
  %236 = phi i32 [ %136, %134 ], [ %234, %232 ]
  %237 = add nuw nsw i64 %135, 1
  %238 = icmp eq i64 %237, 16
  br i1 %238, label %117, label %134

239:                                              ; preds = %139, %239
  %240 = phi i64 [ %242, %239 ], [ %140, %139 ]
  %241 = phi i64 [ %248, %239 ], [ %141, %139 ]
  %242 = add nsw i64 %240, 1
  %243 = getelementptr inbounds i8, i8* %0, i64 %240
  %244 = load i8, i8* %243, align 1, !tbaa !3
  %245 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %241
  %246 = bitcast <2 x i64>* %245 to i8*
  %247 = getelementptr inbounds i8, i8* %246, i64 %135
  store i8 %244, i8* %247, align 1, !tbaa !3
  %248 = add nuw nsw i64 %241, 1
  %249 = icmp eq i64 %248, %30
  br i1 %249, label %232, label %239, !llvm.loop !91

250:                                              ; preds = %282
  %251 = add nuw nsw i32 %116, 16
  %252 = add nsw i64 %114, %40
  %253 = icmp sgt i32 %251, %32
  br i1 %253, label %341, label %113

254:                                              ; preds = %282, %117
  %255 = phi i64 [ 0, %117 ], [ %315, %282 ]
  %256 = phi [2 x <2 x i64>]* [ %28, %117 ], [ %316, %282 ]
  call void @llvm.lifetime.start.p0i8(i64 16, i8* nonnull %36) #2
  store <2 x i64> zeroinitializer, <2 x i64>* %7, align 16, !tbaa !3
  br i1 %35, label %318, label %282

257:                                              ; preds = %318
  %258 = bitcast <2 x i64> %338 to <16 x i8>
  %259 = extractelement <16 x i8> %258, i32 11
  %260 = bitcast <2 x i64> %338 to <16 x i8>
  %261 = extractelement <16 x i8> %260, i32 10
  %262 = bitcast <2 x i64> %338 to <16 x i8>
  %263 = extractelement <16 x i8> %262, i32 9
  %264 = bitcast <2 x i64> %338 to <16 x i8>
  %265 = extractelement <16 x i8> %264, i32 8
  %266 = bitcast <2 x i64> %338 to <16 x i8>
  %267 = extractelement <16 x i8> %266, i32 7
  %268 = bitcast <2 x i64> %338 to <16 x i8>
  %269 = extractelement <16 x i8> %268, i32 6
  %270 = bitcast <2 x i64> %338 to <16 x i8>
  %271 = extractelement <16 x i8> %270, i32 5
  %272 = bitcast <2 x i64> %338 to <16 x i8>
  %273 = extractelement <16 x i8> %272, i32 4
  %274 = bitcast <2 x i64> %338 to <16 x i8>
  %275 = extractelement <16 x i8> %274, i32 3
  %276 = bitcast <2 x i64> %338 to <16 x i8>
  %277 = extractelement <16 x i8> %276, i32 2
  %278 = bitcast <2 x i64> %338 to <16 x i8>
  %279 = extractelement <16 x i8> %278, i32 1
  %280 = bitcast <2 x i64> %338 to <16 x i8>
  %281 = extractelement <16 x i8> %280, i32 0
  br label %282

282:                                              ; preds = %257, %254
  %283 = phi i8 [ %259, %257 ], [ 0, %254 ]
  %284 = phi i8 [ %261, %257 ], [ 0, %254 ]
  %285 = phi i8 [ %263, %257 ], [ 0, %254 ]
  %286 = phi i8 [ %265, %257 ], [ 0, %254 ]
  %287 = phi i8 [ %267, %257 ], [ 0, %254 ]
  %288 = phi i8 [ %269, %257 ], [ 0, %254 ]
  %289 = phi i8 [ %271, %257 ], [ 0, %254 ]
  %290 = phi i8 [ %273, %257 ], [ 0, %254 ]
  %291 = phi i8 [ %275, %257 ], [ 0, %254 ]
  %292 = phi i8 [ %277, %257 ], [ 0, %254 ]
  %293 = phi i8 [ %279, %257 ], [ 0, %254 ]
  %294 = phi i8 [ %281, %257 ], [ 0, %254 ]
  %295 = getelementptr inbounds i8, i8* %118, i64 %255
  store i8 %294, i8* %295, align 1, !tbaa !3
  %296 = getelementptr inbounds i8, i8* %119, i64 %255
  store i8 %293, i8* %296, align 1, !tbaa !3
  %297 = getelementptr inbounds i8, i8* %120, i64 %255
  store i8 %292, i8* %297, align 1, !tbaa !3
  %298 = getelementptr inbounds i8, i8* %121, i64 %255
  store i8 %291, i8* %298, align 1, !tbaa !3
  %299 = getelementptr inbounds i8, i8* %122, i64 %255
  store i8 %290, i8* %299, align 1, !tbaa !3
  %300 = getelementptr inbounds i8, i8* %123, i64 %255
  store i8 %289, i8* %300, align 1, !tbaa !3
  %301 = getelementptr inbounds i8, i8* %124, i64 %255
  store i8 %288, i8* %301, align 1, !tbaa !3
  %302 = getelementptr inbounds i8, i8* %125, i64 %255
  store i8 %287, i8* %302, align 1, !tbaa !3
  %303 = getelementptr inbounds i8, i8* %126, i64 %255
  store i8 %286, i8* %303, align 1, !tbaa !3
  %304 = getelementptr inbounds i8, i8* %127, i64 %255
  store i8 %285, i8* %304, align 1, !tbaa !3
  %305 = getelementptr inbounds i8, i8* %128, i64 %255
  store i8 %284, i8* %305, align 1, !tbaa !3
  %306 = getelementptr inbounds i8, i8* %129, i64 %255
  store i8 %283, i8* %306, align 1, !tbaa !3
  %307 = load i8, i8* %42, align 4, !tbaa !3
  %308 = getelementptr inbounds i8, i8* %130, i64 %255
  store i8 %307, i8* %308, align 1, !tbaa !3
  %309 = load i8, i8* %43, align 1, !tbaa !3
  %310 = getelementptr inbounds i8, i8* %131, i64 %255
  store i8 %309, i8* %310, align 1, !tbaa !3
  %311 = load i8, i8* %44, align 2, !tbaa !3
  %312 = getelementptr inbounds i8, i8* %132, i64 %255
  store i8 %311, i8* %312, align 1, !tbaa !3
  %313 = load i8, i8* %45, align 1, !tbaa !3
  %314 = getelementptr inbounds i8, i8* %133, i64 %255
  store i8 %313, i8* %314, align 1, !tbaa !3
  call void @llvm.lifetime.end.p0i8(i64 16, i8* nonnull %36) #2
  %315 = add nuw nsw i64 %255, 1
  %316 = getelementptr inbounds [2 x <2 x i64>], [2 x <2 x i64>]* %256, i64 %38
  %317 = icmp eq i64 %315, %41
  br i1 %317, label %250, label %254

318:                                              ; preds = %254, %318
  %319 = phi <2 x i64> [ %338, %318 ], [ zeroinitializer, %254 ]
  %320 = phi i64 [ %339, %318 ], [ 0, %254 ]
  %321 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %320
  %322 = load <2 x i64>, <2 x i64>* %321, align 16, !tbaa !3
  %323 = getelementptr inbounds [2 x <2 x i64>], [2 x <2 x i64>]* %256, i64 %320, i64 0
  %324 = bitcast <2 x i64>* %323 to <16 x i8>*
  %325 = load <16 x i8>, <16 x i8>* %324, align 16, !tbaa !3
  %326 = bitcast <2 x i64> %322 to <16 x i8>
  %327 = and <16 x i8> %326, <i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15>
  %328 = tail call <16 x i8> @llvm.x86.ssse3.pshuf.b.128(<16 x i8> %325, <16 x i8> %327) #2
  %329 = lshr <2 x i64> %322, <i64 4, i64 4>
  %330 = getelementptr inbounds [2 x <2 x i64>], [2 x <2 x i64>]* %256, i64 %320, i64 1
  %331 = bitcast <2 x i64>* %330 to <16 x i8>*
  %332 = load <16 x i8>, <16 x i8>* %331, align 16, !tbaa !3
  %333 = bitcast <2 x i64> %329 to <16 x i8>
  %334 = and <16 x i8> %333, <i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15>
  %335 = tail call <16 x i8> @llvm.x86.ssse3.pshuf.b.128(<16 x i8> %332, <16 x i8> %334) #2
  %336 = xor <16 x i8> %335, %328
  %337 = bitcast <16 x i8> %336 to <2 x i64>
  %338 = xor <2 x i64> %319, %337
  store <2 x i64> %338, <2 x i64>* %7, align 16, !tbaa !3
  %339 = add nuw nsw i64 %320, 1
  %340 = icmp eq i64 %339, %30
  br i1 %340, label %257, label %318

341:                                              ; preds = %250
  %342 = and i32 %16, -16
  %343 = shl i64 %252, 32
  %344 = ashr exact i64 %343, 32
  br label %345

345:                                              ; preds = %341, %29
  %346 = phi i32 [ 0, %29 ], [ %342, %341 ]
  %347 = phi i32 [ 0, %29 ], [ %236, %341 ]
  %348 = phi i64 [ 0, %29 ], [ %344, %341 ]
  %349 = icmp sgt i32 %16, %346
  br i1 %349, label %350, label %568

350:                                              ; preds = %345
  %351 = sub i32 %16, %346
  %352 = icmp sgt i32 %351, 0
  br i1 %352, label %353, label %457

353:                                              ; preds = %350
  %354 = icmp sgt i32 %4, 0
  %355 = zext i32 %351 to i64
  %356 = icmp ult i32 %4, 16
  %357 = and i64 %30, 4294967280
  %358 = icmp eq i64 %357, %30
  br label %359

359:                                              ; preds = %475, %353
  %360 = phi i64 [ 0, %353 ], [ %477, %475 ]
  %361 = phi i32 [ %347, %353 ], [ %476, %475 ]
  br i1 %354, label %362, label %475

362:                                              ; preds = %359
  %363 = sext i32 %361 to i64
  br i1 %356, label %364, label %367

364:                                              ; preds = %456, %362
  %365 = phi i64 [ %363, %362 ], [ %368, %456 ]
  %366 = phi i64 [ 0, %362 ], [ %357, %456 ]
  br label %479

367:                                              ; preds = %362
  %368 = add nsw i64 %357, %363
  br label %369

369:                                              ; preds = %369, %367
  %370 = phi i64 [ 0, %367 ], [ %454, %369 ]
  %371 = add i64 %370, %363
  %372 = or i64 %370, 1
  %373 = or i64 %370, 2
  %374 = or i64 %370, 3
  %375 = or i64 %370, 4
  %376 = or i64 %370, 5
  %377 = or i64 %370, 6
  %378 = or i64 %370, 7
  %379 = or i64 %370, 8
  %380 = or i64 %370, 9
  %381 = or i64 %370, 10
  %382 = or i64 %370, 11
  %383 = or i64 %370, 12
  %384 = or i64 %370, 13
  %385 = or i64 %370, 14
  %386 = or i64 %370, 15
  %387 = getelementptr inbounds i8, i8* %0, i64 %371
  %388 = bitcast i8* %387 to <16 x i8>*
  %389 = load <16 x i8>, <16 x i8>* %388, align 1, !tbaa !3
  %390 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %370
  %391 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %372
  %392 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %373
  %393 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %374
  %394 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %375
  %395 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %376
  %396 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %377
  %397 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %378
  %398 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %379
  %399 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %380
  %400 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %381
  %401 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %382
  %402 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %383
  %403 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %384
  %404 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %385
  %405 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %386
  %406 = bitcast <2 x i64>* %390 to i8*
  %407 = bitcast <2 x i64>* %391 to i8*
  %408 = bitcast <2 x i64>* %392 to i8*
  %409 = bitcast <2 x i64>* %393 to i8*
  %410 = bitcast <2 x i64>* %394 to i8*
  %411 = bitcast <2 x i64>* %395 to i8*
  %412 = bitcast <2 x i64>* %396 to i8*
  %413 = bitcast <2 x i64>* %397 to i8*
  %414 = bitcast <2 x i64>* %398 to i8*
  %415 = bitcast <2 x i64>* %399 to i8*
  %416 = bitcast <2 x i64>* %400 to i8*
  %417 = bitcast <2 x i64>* %401 to i8*
  %418 = bitcast <2 x i64>* %402 to i8*
  %419 = bitcast <2 x i64>* %403 to i8*
  %420 = bitcast <2 x i64>* %404 to i8*
  %421 = bitcast <2 x i64>* %405 to i8*
  %422 = getelementptr inbounds i8, i8* %406, i64 %360
  %423 = getelementptr inbounds i8, i8* %407, i64 %360
  %424 = getelementptr inbounds i8, i8* %408, i64 %360
  %425 = getelementptr inbounds i8, i8* %409, i64 %360
  %426 = getelementptr inbounds i8, i8* %410, i64 %360
  %427 = getelementptr inbounds i8, i8* %411, i64 %360
  %428 = getelementptr inbounds i8, i8* %412, i64 %360
  %429 = getelementptr inbounds i8, i8* %413, i64 %360
  %430 = getelementptr inbounds i8, i8* %414, i64 %360
  %431 = getelementptr inbounds i8, i8* %415, i64 %360
  %432 = getelementptr inbounds i8, i8* %416, i64 %360
  %433 = getelementptr inbounds i8, i8* %417, i64 %360
  %434 = getelementptr inbounds i8, i8* %418, i64 %360
  %435 = getelementptr inbounds i8, i8* %419, i64 %360
  %436 = getelementptr inbounds i8, i8* %420, i64 %360
  %437 = getelementptr inbounds i8, i8* %421, i64 %360
  %438 = extractelement <16 x i8> %389, i32 0
  store i8 %438, i8* %422, align 1, !tbaa !3
  %439 = extractelement <16 x i8> %389, i32 1
  store i8 %439, i8* %423, align 1, !tbaa !3
  %440 = extractelement <16 x i8> %389, i32 2
  store i8 %440, i8* %424, align 1, !tbaa !3
  %441 = extractelement <16 x i8> %389, i32 3
  store i8 %441, i8* %425, align 1, !tbaa !3
  %442 = extractelement <16 x i8> %389, i32 4
  store i8 %442, i8* %426, align 1, !tbaa !3
  %443 = extractelement <16 x i8> %389, i32 5
  store i8 %443, i8* %427, align 1, !tbaa !3
  %444 = extractelement <16 x i8> %389, i32 6
  store i8 %444, i8* %428, align 1, !tbaa !3
  %445 = extractelement <16 x i8> %389, i32 7
  store i8 %445, i8* %429, align 1, !tbaa !3
  %446 = extractelement <16 x i8> %389, i32 8
  store i8 %446, i8* %430, align 1, !tbaa !3
  %447 = extractelement <16 x i8> %389, i32 9
  store i8 %447, i8* %431, align 1, !tbaa !3
  %448 = extractelement <16 x i8> %389, i32 10
  store i8 %448, i8* %432, align 1, !tbaa !3
  %449 = extractelement <16 x i8> %389, i32 11
  store i8 %449, i8* %433, align 1, !tbaa !3
  %450 = extractelement <16 x i8> %389, i32 12
  store i8 %450, i8* %434, align 1, !tbaa !3
  %451 = extractelement <16 x i8> %389, i32 13
  store i8 %451, i8* %435, align 1, !tbaa !3
  %452 = extractelement <16 x i8> %389, i32 14
  store i8 %452, i8* %436, align 1, !tbaa !3
  %453 = extractelement <16 x i8> %389, i32 15
  store i8 %453, i8* %437, align 1, !tbaa !3
  %454 = add i64 %370, 16
  %455 = icmp eq i64 %454, %357
  br i1 %455, label %456, label %369, !llvm.loop !93

456:                                              ; preds = %369
  br i1 %358, label %472, label %364

457:                                              ; preds = %475, %350
  %458 = bitcast <2 x i64>* %8 to i8*
  %459 = icmp sgt i32 %4, 0
  %460 = getelementptr inbounds i8, i8* %2, i64 %348
  %461 = sext i32 %5 to i64
  %462 = sext i32 %4 to i64
  %463 = zext i32 %5 to i64
  %464 = zext i32 %351 to i64
  %465 = add nsw i64 %464, -1
  %466 = add nsw i64 %464, -2
  %467 = icmp eq i32 %351, 1
  %468 = and i64 %465, 3
  %469 = icmp ult i64 %466, 3
  %470 = and i64 %465, -4
  %471 = icmp eq i64 %468, 0
  br label %490

472:                                              ; preds = %479, %456
  %473 = phi i64 [ %368, %456 ], [ %482, %479 ]
  %474 = trunc i64 %473 to i32
  br label %475

475:                                              ; preds = %472, %359
  %476 = phi i32 [ %361, %359 ], [ %474, %472 ]
  %477 = add nuw nsw i64 %360, 1
  %478 = icmp eq i64 %477, %355
  br i1 %478, label %457, label %359

479:                                              ; preds = %364, %479
  %480 = phi i64 [ %482, %479 ], [ %365, %364 ]
  %481 = phi i64 [ %488, %479 ], [ %366, %364 ]
  %482 = add nsw i64 %480, 1
  %483 = getelementptr inbounds i8, i8* %0, i64 %480
  %484 = load i8, i8* %483, align 1, !tbaa !3
  %485 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %481
  %486 = bitcast <2 x i64>* %485 to i8*
  %487 = getelementptr inbounds i8, i8* %486, i64 %360
  store i8 %484, i8* %487, align 1, !tbaa !3
  %488 = add nuw nsw i64 %481, 1
  %489 = icmp eq i64 %488, %30
  br i1 %489, label %472, label %479, !llvm.loop !94

490:                                              ; preds = %538, %457
  %491 = phi i64 [ 0, %457 ], [ %539, %538 ]
  %492 = phi [2 x <2 x i64>]* [ %28, %457 ], [ %540, %538 ]
  call void @llvm.lifetime.start.p0i8(i64 16, i8* nonnull %458) #2
  store <2 x i64> zeroinitializer, <2 x i64>* %8, align 16, !tbaa !3
  br i1 %459, label %501, label %496

493:                                              ; preds = %501
  %494 = bitcast <2 x i64> %521 to <16 x i8>
  %495 = extractelement <16 x i8> %494, i32 0
  br label %496

496:                                              ; preds = %493, %490
  %497 = phi i8 [ %495, %493 ], [ 0, %490 ]
  br i1 %352, label %498, label %538

498:                                              ; preds = %496
  %499 = getelementptr inbounds i8, i8* %460, i64 %491
  store i8 %497, i8* %499, align 1, !tbaa !3
  br i1 %467, label %538, label %500

500:                                              ; preds = %498
  br i1 %469, label %524, label %542

501:                                              ; preds = %490, %501
  %502 = phi <2 x i64> [ %521, %501 ], [ zeroinitializer, %490 ]
  %503 = phi i64 [ %522, %501 ], [ 0, %490 ]
  %504 = getelementptr inbounds <2 x i64>, <2 x i64>* %31, i64 %503
  %505 = load <2 x i64>, <2 x i64>* %504, align 16, !tbaa !3
  %506 = getelementptr inbounds [2 x <2 x i64>], [2 x <2 x i64>]* %492, i64 %503, i64 0
  %507 = bitcast <2 x i64>* %506 to <16 x i8>*
  %508 = load <16 x i8>, <16 x i8>* %507, align 16, !tbaa !3
  %509 = bitcast <2 x i64> %505 to <16 x i8>
  %510 = and <16 x i8> %509, <i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15>
  %511 = tail call <16 x i8> @llvm.x86.ssse3.pshuf.b.128(<16 x i8> %508, <16 x i8> %510) #2
  %512 = lshr <2 x i64> %505, <i64 4, i64 4>
  %513 = getelementptr inbounds [2 x <2 x i64>], [2 x <2 x i64>]* %492, i64 %503, i64 1
  %514 = bitcast <2 x i64>* %513 to <16 x i8>*
  %515 = load <16 x i8>, <16 x i8>* %514, align 16, !tbaa !3
  %516 = bitcast <2 x i64> %512 to <16 x i8>
  %517 = and <16 x i8> %516, <i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15>
  %518 = tail call <16 x i8> @llvm.x86.ssse3.pshuf.b.128(<16 x i8> %515, <16 x i8> %517) #2
  %519 = xor <16 x i8> %518, %511
  %520 = bitcast <16 x i8> %519 to <2 x i64>
  %521 = xor <2 x i64> %502, %520
  store <2 x i64> %521, <2 x i64>* %8, align 16, !tbaa !3
  %522 = add nuw nsw i64 %503, 1
  %523 = icmp eq i64 %522, %30
  br i1 %523, label %493, label %501

524:                                              ; preds = %542, %500
  %525 = phi i64 [ 1, %500 ], [ %565, %542 ]
  %526 = phi i8* [ %460, %500 ], [ %561, %542 ]
  br i1 %471, label %538, label %527

527:                                              ; preds = %524, %527
  %528 = phi i64 [ %535, %527 ], [ %525, %524 ]
  %529 = phi i8* [ %531, %527 ], [ %526, %524 ]
  %530 = phi i64 [ %536, %527 ], [ %468, %524 ]
  %531 = getelementptr inbounds i8, i8* %529, i64 %461
  %532 = getelementptr inbounds i8, i8* %458, i64 %528
  %533 = load i8, i8* %532, align 1, !tbaa !3
  %534 = getelementptr inbounds i8, i8* %531, i64 %491
  store i8 %533, i8* %534, align 1, !tbaa !3
  %535 = add nuw nsw i64 %528, 1
  %536 = add i64 %530, -1
  %537 = icmp eq i64 %536, 0
  br i1 %537, label %538, label %527, !llvm.loop !95

538:                                              ; preds = %524, %527, %498, %496
  call void @llvm.lifetime.end.p0i8(i64 16, i8* nonnull %458) #2
  %539 = add nuw nsw i64 %491, 1
  %540 = getelementptr inbounds [2 x <2 x i64>], [2 x <2 x i64>]* %492, i64 %462
  %541 = icmp eq i64 %539, %463
  br i1 %541, label %568, label %490

542:                                              ; preds = %500, %542
  %543 = phi i64 [ %565, %542 ], [ 1, %500 ]
  %544 = phi i8* [ %561, %542 ], [ %460, %500 ]
  %545 = phi i64 [ %566, %542 ], [ %470, %500 ]
  %546 = getelementptr inbounds i8, i8* %544, i64 %461
  %547 = getelementptr inbounds i8, i8* %458, i64 %543
  %548 = load i8, i8* %547, align 1, !tbaa !3
  %549 = getelementptr inbounds i8, i8* %546, i64 %491
  store i8 %548, i8* %549, align 1, !tbaa !3
  %550 = add nuw nsw i64 %543, 1
  %551 = getelementptr inbounds i8, i8* %546, i64 %461
  %552 = getelementptr inbounds i8, i8* %458, i64 %550
  %553 = load i8, i8* %552, align 1, !tbaa !3
  %554 = getelementptr inbounds i8, i8* %551, i64 %491
  store i8 %553, i8* %554, align 1, !tbaa !3
  %555 = add nuw nsw i64 %543, 2
  %556 = getelementptr inbounds i8, i8* %551, i64 %461
  %557 = getelementptr inbounds i8, i8* %458, i64 %555
  %558 = load i8, i8* %557, align 1, !tbaa !3
  %559 = getelementptr inbounds i8, i8* %556, i64 %491
  store i8 %558, i8* %559, align 1, !tbaa !3
  %560 = add nuw nsw i64 %543, 3
  %561 = getelementptr inbounds i8, i8* %556, i64 %461
  %562 = getelementptr inbounds i8, i8* %458, i64 %560
  %563 = load i8, i8* %562, align 1, !tbaa !3
  %564 = getelementptr inbounds i8, i8* %561, i64 %491
  store i8 %563, i8* %564, align 1, !tbaa !3
  %565 = add nuw nsw i64 %543, 4
  %566 = add i64 %545, -4
  %567 = icmp eq i64 %566, 0
  br i1 %567, label %524, label %542

568:                                              ; preds = %538, %345
  tail call void @llvm.stackrestore(i8* %27)
  br label %569

569:                                              ; preds = %568, %22, %18, %14, %11, %6
  %570 = phi i32 [ 1, %6 ], [ 2, %11 ], [ 3, %14 ], [ 4, %18 ], [ 0, %568 ], [ 5, %22 ]
  ret i32 %570
}

; Function Attrs: nounwind uwtable
define dso_local i32 @ec_decode_ssse3(i8* noalias nocapture %0, i32 %1, i32 %2, [2 x i32]* noalias nocapture readonly %3, i32 %4) local_unnamed_addr #4 personality i32 (...)* @__CxxFrameHandler3 {
  %6 = alloca [32 x i8], align 16
  %7 = alloca <2 x i64>, align 16
  %8 = alloca <2 x i64>, align 16
  %9 = add i32 %2, -1
  %10 = icmp ugt i32 %9, 253
  br i1 %10, label %686, label %11

11:                                               ; preds = %5
  %12 = add i32 %4, -1
  %13 = icmp ugt i32 %12, 253
  br i1 %13, label %686, label %14

14:                                               ; preds = %11
  %15 = srem i32 %1, %2
  %16 = sdiv i32 %1, %2
  %17 = icmp eq i32 %15, 0
  br i1 %17, label %18, label %686

18:                                               ; preds = %14
  %19 = zext i32 %4 to i64
  br label %22

20:                                               ; preds = %29
  %21 = icmp eq i64 %33, %19
  br i1 %21, label %34, label %22

22:                                               ; preds = %20, %18
  %23 = phi i64 [ 0, %18 ], [ %33, %20 ]
  %24 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %23, i64 0
  %25 = load i32, i32* %24, align 4, !tbaa !22
  %26 = icmp sgt i32 %25, -1
  %27 = icmp slt i32 %25, %2
  %28 = and i1 %26, %27
  br i1 %28, label %29, label %686

29:                                               ; preds = %22
  %30 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %23, i64 1
  %31 = load i32, i32* %30, align 4, !tbaa !22
  %32 = icmp ugt i32 %31, 253
  %33 = add nuw nsw i64 %23, 1
  br i1 %32, label %686, label %20

34:                                               ; preds = %20
  %35 = getelementptr inbounds [32 x i8], [32 x i8]* %6, i64 0, i64 0
  call void @llvm.lifetime.start.p0i8(i64 32, i8* nonnull %35) #2
  call void @llvm.memset.p0i8.i64(i8* nonnull align 16 dereferenceable(32) %35, i8 0, i64 32, i1 false) #2
  %36 = zext i32 %4 to i64
  br label %37

37:                                               ; preds = %50, %34
  %38 = phi i64 [ 0, %34 ], [ %55, %50 ]
  %39 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %38, i64 0
  %40 = load i32, i32* %39, align 4, !tbaa !22
  %41 = sext i32 %40 to i64
  %42 = lshr i64 %41, 3
  %43 = getelementptr inbounds [32 x i8], [32 x i8]* %6, i64 0, i64 %42
  %44 = load i8, i8* %43, align 1, !tbaa !3
  %45 = zext i8 %44 to i32
  %46 = and i32 %40, 7
  %47 = shl nuw nsw i32 1, %46
  %48 = and i32 %47, %45
  %49 = icmp eq i32 %48, 0
  br i1 %49, label %50, label %57

50:                                               ; preds = %37
  %51 = trunc i32 %40 to i8
  %52 = and i8 %51, 7
  %53 = shl nuw i8 1, %52
  %54 = or i8 %53, %44
  store i8 %54, i8* %43, align 1, !tbaa !3
  %55 = add nuw nsw i64 %38, 1
  %56 = icmp eq i64 %55, %36
  br i1 %56, label %58, label %37

57:                                               ; preds = %37
  call void @llvm.lifetime.end.p0i8(i64 32, i8* nonnull %35) #2
  br label %686

58:                                               ; preds = %50
  call void @llvm.lifetime.end.p0i8(i64 32, i8* nonnull %35) #2
  call void @llvm.lifetime.start.p0i8(i64 32, i8* nonnull %35) #2
  call void @llvm.memset.p0i8.i64(i8* nonnull align 16 dereferenceable(32) %35, i8 0, i64 32, i1 false) #2
  br label %59

59:                                               ; preds = %72, %58
  %60 = phi i64 [ 0, %58 ], [ %77, %72 ]
  %61 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %60, i64 1
  %62 = load i32, i32* %61, align 4, !tbaa !22
  %63 = sext i32 %62 to i64
  %64 = lshr i64 %63, 3
  %65 = getelementptr inbounds [32 x i8], [32 x i8]* %6, i64 0, i64 %64
  %66 = load i8, i8* %65, align 1, !tbaa !3
  %67 = zext i8 %66 to i32
  %68 = and i32 %62, 7
  %69 = shl nuw nsw i32 1, %68
  %70 = and i32 %69, %67
  %71 = icmp eq i32 %70, 0
  br i1 %71, label %72, label %79

72:                                               ; preds = %59
  %73 = trunc i32 %62 to i8
  %74 = and i8 %73, 7
  %75 = shl nuw i8 1, %74
  %76 = or i8 %75, %66
  store i8 %76, i8* %65, align 1, !tbaa !3
  %77 = add nuw nsw i64 %60, 1
  %78 = icmp eq i64 %77, %36
  br i1 %78, label %80, label %59

79:                                               ; preds = %59
  call void @llvm.lifetime.end.p0i8(i64 32, i8* nonnull %35) #2
  br label %686

80:                                               ; preds = %72
  call void @llvm.lifetime.end.p0i8(i64 32, i8* nonnull %35) #2
  %81 = mul nsw i32 %2, %2
  %82 = zext i32 %81 to i64
  %83 = tail call i8* @llvm.stacksave()
  %84 = alloca %struct.GF, i64 %82, align 16
  %85 = getelementptr inbounds %struct.GF, %struct.GF* %84, i64 0, i32 0
  call void @llvm.memset.p0i8.i64(i8* nonnull align 16 %85, i8 0, i64 %82, i1 false)
  %86 = icmp sgt i32 %2, 0
  br i1 %86, label %87, label %107

87:                                               ; preds = %80
  %88 = zext i32 %2 to i64
  %89 = zext i32 %2 to i64
  %90 = add nsw i64 %89, -1
  %91 = and i64 %89, 3
  %92 = icmp ult i64 %90, 3
  br i1 %92, label %95, label %93

93:                                               ; preds = %87
  %94 = and i64 %89, 4294967292
  br label %111

95:                                               ; preds = %111, %87
  %96 = phi i64 [ 0, %87 ], [ %129, %111 ]
  %97 = icmp eq i64 %91, 0
  br i1 %97, label %107, label %98

98:                                               ; preds = %95, %98
  %99 = phi i64 [ %104, %98 ], [ %96, %95 ]
  %100 = phi i64 [ %105, %98 ], [ %91, %95 ]
  %101 = mul nsw i64 %99, %88
  %102 = add nuw nsw i64 %101, %99
  %103 = getelementptr inbounds %struct.GF, %struct.GF* %84, i64 %102, i32 0
  store i8 1, i8* %103, align 1, !tbaa !3
  %104 = add nuw nsw i64 %99, 1
  %105 = add i64 %100, -1
  %106 = icmp eq i64 %105, 0
  br i1 %106, label %107, label %98, !llvm.loop !96

107:                                              ; preds = %95, %98, %80
  %108 = zext i32 %4 to i64
  %109 = zext i32 %2 to i64
  %110 = icmp eq i32 %2, 1
  br label %138

111:                                              ; preds = %111, %93
  %112 = phi i64 [ 0, %93 ], [ %129, %111 ]
  %113 = phi i64 [ %94, %93 ], [ %130, %111 ]
  %114 = mul nsw i64 %112, %88
  %115 = add nuw nsw i64 %114, %112
  %116 = getelementptr inbounds %struct.GF, %struct.GF* %84, i64 %115, i32 0
  store i8 1, i8* %116, align 4, !tbaa !3
  %117 = or i64 %112, 1
  %118 = mul nsw i64 %117, %88
  %119 = add nuw nsw i64 %118, %117
  %120 = getelementptr inbounds %struct.GF, %struct.GF* %84, i64 %119, i32 0
  store i8 1, i8* %120, align 1, !tbaa !3
  %121 = or i64 %112, 2
  %122 = mul nsw i64 %121, %88
  %123 = add nuw nsw i64 %122, %121
  %124 = getelementptr inbounds %struct.GF, %struct.GF* %84, i64 %123, i32 0
  store i8 1, i8* %124, align 2, !tbaa !3
  %125 = or i64 %112, 3
  %126 = mul nsw i64 %125, %88
  %127 = add nuw nsw i64 %126, %125
  %128 = getelementptr inbounds %struct.GF, %struct.GF* %84, i64 %127, i32 0
  store i8 1, i8* %128, align 1, !tbaa !3
  %129 = add nuw nsw i64 %112, 4
  %130 = add i64 %113, -4
  %131 = icmp eq i64 %130, 0
  br i1 %131, label %95, label %111

132:                                              ; preds = %159
  call fastcc void @"?mat_inverse@@YAXPEAUGF@@H@Z"(%struct.GF* nonnull %84, i32 %2) #2
  %133 = mul nsw i32 %4, %2
  %134 = zext i32 %133 to i64
  %135 = alloca [2 x <2 x i64>], i64 %134, align 16
  %136 = zext i32 %4 to i64
  %137 = zext i32 %2 to i64
  br label %177

138:                                              ; preds = %159, %107
  %139 = phi i64 [ 0, %107 ], [ %160, %159 ]
  %140 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %139, i64 0
  %141 = load i32, i32* %140, align 4, !tbaa !22
  %142 = mul nsw i32 %141, %2
  %143 = sext i32 %142 to i64
  %144 = getelementptr inbounds %struct.GF, %struct.GF* %84, i64 %143
  %145 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %139, i64 1
  %146 = load i32, i32* %145, align 4, !tbaa !22
  %147 = srem i32 %146, 255
  br i1 %86, label %148, label %159

148:                                              ; preds = %138
  %149 = icmp slt i32 %147, 0
  %150 = add nsw i32 %147, 255
  %151 = select i1 %149, i32 %150, i32 %147
  %152 = add nuw nsw i32 %151, 1
  %153 = zext i32 %152 to i64
  %154 = getelementptr inbounds [256 x i8], [256 x i8]* @"?value_table@GF@@0QBEB", i64 0, i64 %153
  %155 = load i8, i8* %154, align 1, !tbaa !3, !noalias !97
  %156 = zext i8 %155 to i64
  %157 = getelementptr inbounds [256 x i8], [256 x i8]* @"?exp_table@GF@@0QBEB", i64 0, i64 %156
  %158 = getelementptr %struct.GF, %struct.GF* %144, i64 0, i32 0
  store i8 1, i8* %158, align 1, !tbaa !3
  br i1 %110, label %159, label %162

159:                                              ; preds = %148, %162, %138
  %160 = add nuw nsw i64 %139, 1
  %161 = icmp eq i64 %160, %108
  br i1 %161, label %132, label %138

162:                                              ; preds = %148, %162
  %163 = phi i64 [ %175, %162 ], [ 1, %148 ]
  %164 = load i8, i8* %157, align 1, !tbaa !3, !noalias !100
  %165 = zext i8 %164 to i32
  %166 = trunc i64 %163 to i32
  %167 = mul nsw i32 %166, %165
  %168 = urem i32 %167, 255
  %169 = add nuw nsw i32 %168, 1
  %170 = zext i32 %169 to i64
  %171 = getelementptr inbounds [256 x i8], [256 x i8]* @"?value_table@GF@@0QBEB", i64 0, i64 %170
  %172 = load i8, i8* %171, align 1, !tbaa !3, !noalias !103
  %173 = getelementptr inbounds %struct.GF, %struct.GF* %144, i64 %163
  %174 = getelementptr %struct.GF, %struct.GF* %173, i64 0, i32 0
  store i8 %172, i8* %174, align 1, !tbaa !3
  %175 = add nuw nsw i64 %163, 1
  %176 = icmp eq i64 %175, %109
  br i1 %176, label %159, label %162, !llvm.loop !106

177:                                              ; preds = %216, %132
  %178 = phi i64 [ 0, %132 ], [ %218, %216 ]
  %179 = phi i32 [ 0, %132 ], [ %217, %216 ]
  br i1 %86, label %180, label %216

180:                                              ; preds = %177
  %181 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %178, i64 0
  %182 = load i32, i32* %181, align 4, !tbaa !22
  %183 = mul nsw i32 %182, %2
  %184 = sext i32 %179 to i64
  %185 = sext i32 %183 to i64
  br label %203

186:                                              ; preds = %216
  %187 = zext i32 %2 to i64
  %188 = alloca <2 x i64>, i64 %187, align 16
  %189 = icmp slt i32 %16, 16
  br i1 %189, label %466, label %190

190:                                              ; preds = %186
  %191 = add nsw i32 %16, -16
  %192 = bitcast <2 x i64>* %7 to i8*
  %193 = sext i32 %2 to i64
  %194 = sext i32 %191 to i64
  %195 = zext i32 %4 to i64
  %196 = getelementptr inbounds i8, i8* %192, i64 12
  %197 = getelementptr inbounds i8, i8* %192, i64 13
  %198 = getelementptr inbounds i8, i8* %192, i64 14
  %199 = getelementptr inbounds i8, i8* %192, i64 15
  %200 = icmp ult i32 %2, 16
  %201 = and i64 %109, 4294967280
  %202 = icmp eq i64 %201, %109
  br label %249

203:                                              ; preds = %220, %180
  %204 = phi i64 [ 0, %180 ], [ %221, %220 ]
  %205 = phi i64 [ %184, %180 ], [ %222, %220 ]
  %206 = add nsw i64 %204, %185
  %207 = getelementptr inbounds %struct.GF, %struct.GF* %84, i64 %206, i32 0
  %208 = load i8, i8* %207, align 1, !tbaa.struct !19
  %209 = zext i8 %208 to i64
  %210 = getelementptr inbounds [2 x <2 x i64>], [2 x <2 x i64>]* %135, i64 %205
  %211 = bitcast [2 x <2 x i64>]* %210 to i8*
  %212 = getelementptr inbounds [2 x <2 x i64>], [2 x <2 x i64>]* %135, i64 %205, i64 1
  %213 = bitcast <2 x i64>* %212 to i8*
  br label %224

214:                                              ; preds = %220
  %215 = trunc i64 %222 to i32
  br label %216

216:                                              ; preds = %214, %177
  %217 = phi i32 [ %179, %177 ], [ %215, %214 ]
  %218 = add nuw nsw i64 %178, 1
  %219 = icmp eq i64 %218, %136
  br i1 %219, label %186, label %177

220:                                              ; preds = %224
  %221 = add nuw nsw i64 %204, 1
  %222 = add nsw i64 %205, 1
  %223 = icmp eq i64 %221, %137
  br i1 %223, label %214, label %203

224:                                              ; preds = %224, %203
  %225 = phi i64 [ 0, %203 ], [ %247, %224 ]
  %226 = shl i64 %225, 8
  %227 = or i64 %226, %209
  %228 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %227, i32 0
  %229 = load i8, i8* %228, align 1, !tbaa !3, !noalias !107
  %230 = getelementptr inbounds i8, i8* %211, i64 %225
  store i8 %229, i8* %230, align 2, !tbaa !3
  %231 = shl nuw nsw i64 %225, 12
  %232 = or i64 %231, %209
  %233 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %232, i32 0
  %234 = load i8, i8* %233, align 1, !tbaa !3, !noalias !110
  %235 = getelementptr inbounds i8, i8* %213, i64 %225
  store i8 %234, i8* %235, align 2, !tbaa !3
  %236 = or i64 %225, 1
  %237 = shl i64 %236, 8
  %238 = or i64 %237, %209
  %239 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %238, i32 0
  %240 = load i8, i8* %239, align 1, !tbaa !3, !noalias !107
  %241 = getelementptr inbounds i8, i8* %211, i64 %236
  store i8 %240, i8* %241, align 1, !tbaa !3
  %242 = shl nuw nsw i64 %236, 12
  %243 = or i64 %242, %209
  %244 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %243, i32 0
  %245 = load i8, i8* %244, align 1, !tbaa !3, !noalias !110
  %246 = getelementptr inbounds i8, i8* %213, i64 %236
  store i8 %245, i8* %246, align 1, !tbaa !3
  %247 = add nuw nsw i64 %225, 2
  %248 = icmp eq i64 %247, 16
  br i1 %248, label %220, label %224

249:                                              ; preds = %190, %371
  %250 = phi i64 [ 0, %190 ], [ %372, %371 ]
  %251 = phi i32 [ 0, %190 ], [ %357, %371 ]
  br label %255

252:                                              ; preds = %356
  %253 = mul nsw i64 %250, %193
  %254 = getelementptr inbounds i8, i8* %0, i64 %253
  br label %374

255:                                              ; preds = %356, %249
  %256 = phi i64 [ 0, %249 ], [ %358, %356 ]
  %257 = phi i32 [ %251, %249 ], [ %357, %356 ]
  br i1 %86, label %258, label %356

258:                                              ; preds = %255
  %259 = sext i32 %257 to i64
  br i1 %200, label %260, label %263

260:                                              ; preds = %352, %258
  %261 = phi i64 [ %259, %258 ], [ %264, %352 ]
  %262 = phi i64 [ 0, %258 ], [ %201, %352 ]
  br label %360

263:                                              ; preds = %258
  %264 = add nsw i64 %201, %259
  br label %265

265:                                              ; preds = %265, %263
  %266 = phi i64 [ 0, %263 ], [ %350, %265 ]
  %267 = add i64 %266, %259
  %268 = or i64 %266, 1
  %269 = or i64 %266, 2
  %270 = or i64 %266, 3
  %271 = or i64 %266, 4
  %272 = or i64 %266, 5
  %273 = or i64 %266, 6
  %274 = or i64 %266, 7
  %275 = or i64 %266, 8
  %276 = or i64 %266, 9
  %277 = or i64 %266, 10
  %278 = or i64 %266, 11
  %279 = or i64 %266, 12
  %280 = or i64 %266, 13
  %281 = or i64 %266, 14
  %282 = or i64 %266, 15
  %283 = getelementptr inbounds i8, i8* %0, i64 %267
  %284 = bitcast i8* %283 to <16 x i8>*
  %285 = load <16 x i8>, <16 x i8>* %284, align 1, !tbaa !3
  %286 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %266
  %287 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %268
  %288 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %269
  %289 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %270
  %290 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %271
  %291 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %272
  %292 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %273
  %293 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %274
  %294 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %275
  %295 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %276
  %296 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %277
  %297 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %278
  %298 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %279
  %299 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %280
  %300 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %281
  %301 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %282
  %302 = bitcast <2 x i64>* %286 to i8*
  %303 = bitcast <2 x i64>* %287 to i8*
  %304 = bitcast <2 x i64>* %288 to i8*
  %305 = bitcast <2 x i64>* %289 to i8*
  %306 = bitcast <2 x i64>* %290 to i8*
  %307 = bitcast <2 x i64>* %291 to i8*
  %308 = bitcast <2 x i64>* %292 to i8*
  %309 = bitcast <2 x i64>* %293 to i8*
  %310 = bitcast <2 x i64>* %294 to i8*
  %311 = bitcast <2 x i64>* %295 to i8*
  %312 = bitcast <2 x i64>* %296 to i8*
  %313 = bitcast <2 x i64>* %297 to i8*
  %314 = bitcast <2 x i64>* %298 to i8*
  %315 = bitcast <2 x i64>* %299 to i8*
  %316 = bitcast <2 x i64>* %300 to i8*
  %317 = bitcast <2 x i64>* %301 to i8*
  %318 = getelementptr inbounds i8, i8* %302, i64 %256
  %319 = getelementptr inbounds i8, i8* %303, i64 %256
  %320 = getelementptr inbounds i8, i8* %304, i64 %256
  %321 = getelementptr inbounds i8, i8* %305, i64 %256
  %322 = getelementptr inbounds i8, i8* %306, i64 %256
  %323 = getelementptr inbounds i8, i8* %307, i64 %256
  %324 = getelementptr inbounds i8, i8* %308, i64 %256
  %325 = getelementptr inbounds i8, i8* %309, i64 %256
  %326 = getelementptr inbounds i8, i8* %310, i64 %256
  %327 = getelementptr inbounds i8, i8* %311, i64 %256
  %328 = getelementptr inbounds i8, i8* %312, i64 %256
  %329 = getelementptr inbounds i8, i8* %313, i64 %256
  %330 = getelementptr inbounds i8, i8* %314, i64 %256
  %331 = getelementptr inbounds i8, i8* %315, i64 %256
  %332 = getelementptr inbounds i8, i8* %316, i64 %256
  %333 = getelementptr inbounds i8, i8* %317, i64 %256
  %334 = extractelement <16 x i8> %285, i32 0
  store i8 %334, i8* %318, align 1, !tbaa !3
  %335 = extractelement <16 x i8> %285, i32 1
  store i8 %335, i8* %319, align 1, !tbaa !3
  %336 = extractelement <16 x i8> %285, i32 2
  store i8 %336, i8* %320, align 1, !tbaa !3
  %337 = extractelement <16 x i8> %285, i32 3
  store i8 %337, i8* %321, align 1, !tbaa !3
  %338 = extractelement <16 x i8> %285, i32 4
  store i8 %338, i8* %322, align 1, !tbaa !3
  %339 = extractelement <16 x i8> %285, i32 5
  store i8 %339, i8* %323, align 1, !tbaa !3
  %340 = extractelement <16 x i8> %285, i32 6
  store i8 %340, i8* %324, align 1, !tbaa !3
  %341 = extractelement <16 x i8> %285, i32 7
  store i8 %341, i8* %325, align 1, !tbaa !3
  %342 = extractelement <16 x i8> %285, i32 8
  store i8 %342, i8* %326, align 1, !tbaa !3
  %343 = extractelement <16 x i8> %285, i32 9
  store i8 %343, i8* %327, align 1, !tbaa !3
  %344 = extractelement <16 x i8> %285, i32 10
  store i8 %344, i8* %328, align 1, !tbaa !3
  %345 = extractelement <16 x i8> %285, i32 11
  store i8 %345, i8* %329, align 1, !tbaa !3
  %346 = extractelement <16 x i8> %285, i32 12
  store i8 %346, i8* %330, align 1, !tbaa !3
  %347 = extractelement <16 x i8> %285, i32 13
  store i8 %347, i8* %331, align 1, !tbaa !3
  %348 = extractelement <16 x i8> %285, i32 14
  store i8 %348, i8* %332, align 1, !tbaa !3
  %349 = extractelement <16 x i8> %285, i32 15
  store i8 %349, i8* %333, align 1, !tbaa !3
  %350 = add i64 %266, 16
  %351 = icmp eq i64 %350, %201
  br i1 %351, label %352, label %265, !llvm.loop !113

352:                                              ; preds = %265
  br i1 %202, label %353, label %260

353:                                              ; preds = %360, %352
  %354 = phi i64 [ %264, %352 ], [ %363, %360 ]
  %355 = trunc i64 %354 to i32
  br label %356

356:                                              ; preds = %353, %255
  %357 = phi i32 [ %257, %255 ], [ %355, %353 ]
  %358 = add nuw nsw i64 %256, 1
  %359 = icmp eq i64 %358, 16
  br i1 %359, label %252, label %255

360:                                              ; preds = %260, %360
  %361 = phi i64 [ %363, %360 ], [ %261, %260 ]
  %362 = phi i64 [ %369, %360 ], [ %262, %260 ]
  %363 = add nsw i64 %361, 1
  %364 = getelementptr inbounds i8, i8* %0, i64 %361
  %365 = load i8, i8* %364, align 1, !tbaa !3
  %366 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %362
  %367 = bitcast <2 x i64>* %366 to i8*
  %368 = getelementptr inbounds i8, i8* %367, i64 %256
  store i8 %365, i8* %368, align 1, !tbaa !3
  %369 = add nuw nsw i64 %362, 1
  %370 = icmp eq i64 %369, %187
  br i1 %370, label %353, label %360, !llvm.loop !114

371:                                              ; preds = %402
  %372 = add nuw nsw i64 %250, 16
  %373 = icmp sgt i64 %372, %194
  br i1 %373, label %464, label %249

374:                                              ; preds = %402, %252
  %375 = phi i64 [ 0, %252 ], [ %438, %402 ]
  %376 = phi [2 x <2 x i64>]* [ %135, %252 ], [ %439, %402 ]
  call void @llvm.lifetime.start.p0i8(i64 16, i8* nonnull %192) #2
  store <2 x i64> zeroinitializer, <2 x i64>* %7, align 16, !tbaa !3
  br i1 %86, label %441, label %402

377:                                              ; preds = %441
  %378 = bitcast <2 x i64> %461 to <16 x i8>
  %379 = extractelement <16 x i8> %378, i32 11
  %380 = bitcast <2 x i64> %461 to <16 x i8>
  %381 = extractelement <16 x i8> %380, i32 10
  %382 = bitcast <2 x i64> %461 to <16 x i8>
  %383 = extractelement <16 x i8> %382, i32 9
  %384 = bitcast <2 x i64> %461 to <16 x i8>
  %385 = extractelement <16 x i8> %384, i32 8
  %386 = bitcast <2 x i64> %461 to <16 x i8>
  %387 = extractelement <16 x i8> %386, i32 7
  %388 = bitcast <2 x i64> %461 to <16 x i8>
  %389 = extractelement <16 x i8> %388, i32 6
  %390 = bitcast <2 x i64> %461 to <16 x i8>
  %391 = extractelement <16 x i8> %390, i32 5
  %392 = bitcast <2 x i64> %461 to <16 x i8>
  %393 = extractelement <16 x i8> %392, i32 4
  %394 = bitcast <2 x i64> %461 to <16 x i8>
  %395 = extractelement <16 x i8> %394, i32 3
  %396 = bitcast <2 x i64> %461 to <16 x i8>
  %397 = extractelement <16 x i8> %396, i32 2
  %398 = bitcast <2 x i64> %461 to <16 x i8>
  %399 = extractelement <16 x i8> %398, i32 1
  %400 = bitcast <2 x i64> %461 to <16 x i8>
  %401 = extractelement <16 x i8> %400, i32 0
  br label %402

402:                                              ; preds = %377, %374
  %403 = phi i8 [ %379, %377 ], [ 0, %374 ]
  %404 = phi i8 [ %381, %377 ], [ 0, %374 ]
  %405 = phi i8 [ %383, %377 ], [ 0, %374 ]
  %406 = phi i8 [ %385, %377 ], [ 0, %374 ]
  %407 = phi i8 [ %387, %377 ], [ 0, %374 ]
  %408 = phi i8 [ %389, %377 ], [ 0, %374 ]
  %409 = phi i8 [ %391, %377 ], [ 0, %374 ]
  %410 = phi i8 [ %393, %377 ], [ 0, %374 ]
  %411 = phi i8 [ %395, %377 ], [ 0, %374 ]
  %412 = phi i8 [ %397, %377 ], [ 0, %374 ]
  %413 = phi i8 [ %399, %377 ], [ 0, %374 ]
  %414 = phi i8 [ %401, %377 ], [ 0, %374 ]
  %415 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %375, i64 0
  %416 = load i32, i32* %415, align 4, !tbaa !22
  %417 = sext i32 %416 to i64
  %418 = getelementptr inbounds i8, i8* %254, i64 %417
  store i8 %414, i8* %418, align 1, !tbaa !3
  %419 = getelementptr inbounds i8, i8* %418, i64 %193
  store i8 %413, i8* %419, align 1, !tbaa !3
  %420 = getelementptr inbounds i8, i8* %419, i64 %193
  store i8 %412, i8* %420, align 1, !tbaa !3
  %421 = getelementptr inbounds i8, i8* %420, i64 %193
  store i8 %411, i8* %421, align 1, !tbaa !3
  %422 = getelementptr inbounds i8, i8* %421, i64 %193
  store i8 %410, i8* %422, align 1, !tbaa !3
  %423 = getelementptr inbounds i8, i8* %422, i64 %193
  store i8 %409, i8* %423, align 1, !tbaa !3
  %424 = getelementptr inbounds i8, i8* %423, i64 %193
  store i8 %408, i8* %424, align 1, !tbaa !3
  %425 = getelementptr inbounds i8, i8* %424, i64 %193
  store i8 %407, i8* %425, align 1, !tbaa !3
  %426 = getelementptr inbounds i8, i8* %425, i64 %193
  store i8 %406, i8* %426, align 1, !tbaa !3
  %427 = getelementptr inbounds i8, i8* %426, i64 %193
  store i8 %405, i8* %427, align 1, !tbaa !3
  %428 = getelementptr inbounds i8, i8* %427, i64 %193
  store i8 %404, i8* %428, align 1, !tbaa !3
  %429 = getelementptr inbounds i8, i8* %428, i64 %193
  store i8 %403, i8* %429, align 1, !tbaa !3
  %430 = getelementptr inbounds i8, i8* %429, i64 %193
  %431 = load i8, i8* %196, align 4, !tbaa !3
  store i8 %431, i8* %430, align 1, !tbaa !3
  %432 = getelementptr inbounds i8, i8* %430, i64 %193
  %433 = load i8, i8* %197, align 1, !tbaa !3
  store i8 %433, i8* %432, align 1, !tbaa !3
  %434 = getelementptr inbounds i8, i8* %432, i64 %193
  %435 = load i8, i8* %198, align 2, !tbaa !3
  store i8 %435, i8* %434, align 1, !tbaa !3
  %436 = getelementptr inbounds i8, i8* %434, i64 %193
  %437 = load i8, i8* %199, align 1, !tbaa !3
  store i8 %437, i8* %436, align 1, !tbaa !3
  call void @llvm.lifetime.end.p0i8(i64 16, i8* nonnull %192) #2
  %438 = add nuw nsw i64 %375, 1
  %439 = getelementptr inbounds [2 x <2 x i64>], [2 x <2 x i64>]* %376, i64 %193
  %440 = icmp eq i64 %438, %195
  br i1 %440, label %371, label %374

441:                                              ; preds = %374, %441
  %442 = phi <2 x i64> [ %461, %441 ], [ zeroinitializer, %374 ]
  %443 = phi i64 [ %462, %441 ], [ 0, %374 ]
  %444 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %443
  %445 = load <2 x i64>, <2 x i64>* %444, align 16, !tbaa !3
  %446 = getelementptr inbounds [2 x <2 x i64>], [2 x <2 x i64>]* %376, i64 %443, i64 0
  %447 = bitcast <2 x i64>* %446 to <16 x i8>*
  %448 = load <16 x i8>, <16 x i8>* %447, align 16, !tbaa !3
  %449 = bitcast <2 x i64> %445 to <16 x i8>
  %450 = and <16 x i8> %449, <i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15>
  %451 = call <16 x i8> @llvm.x86.ssse3.pshuf.b.128(<16 x i8> %448, <16 x i8> %450) #2
  %452 = lshr <2 x i64> %445, <i64 4, i64 4>
  %453 = getelementptr inbounds [2 x <2 x i64>], [2 x <2 x i64>]* %376, i64 %443, i64 1
  %454 = bitcast <2 x i64>* %453 to <16 x i8>*
  %455 = load <16 x i8>, <16 x i8>* %454, align 16, !tbaa !3
  %456 = bitcast <2 x i64> %452 to <16 x i8>
  %457 = and <16 x i8> %456, <i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15>
  %458 = call <16 x i8> @llvm.x86.ssse3.pshuf.b.128(<16 x i8> %455, <16 x i8> %457) #2
  %459 = xor <16 x i8> %458, %451
  %460 = bitcast <16 x i8> %459 to <2 x i64>
  %461 = xor <2 x i64> %442, %460
  store <2 x i64> %461, <2 x i64>* %7, align 16, !tbaa !3
  %462 = add nuw nsw i64 %443, 1
  %463 = icmp eq i64 %462, %187
  br i1 %463, label %377, label %441

464:                                              ; preds = %371
  %465 = trunc i64 %372 to i32
  br label %466

466:                                              ; preds = %464, %186
  %467 = phi i32 [ 0, %186 ], [ %465, %464 ]
  %468 = phi i32 [ 0, %186 ], [ %357, %464 ]
  %469 = icmp sgt i32 %16, %467
  br i1 %469, label %470, label %685

470:                                              ; preds = %466
  %471 = sub i32 %16, %467
  %472 = icmp sgt i32 %471, 0
  br i1 %472, label %473, label %576

473:                                              ; preds = %470
  %474 = zext i32 %471 to i64
  %475 = icmp ult i32 %2, 16
  %476 = and i64 %109, 4294967280
  %477 = icmp eq i64 %476, %109
  br label %478

478:                                              ; preds = %594, %473
  %479 = phi i64 [ 0, %473 ], [ %596, %594 ]
  %480 = phi i32 [ %468, %473 ], [ %595, %594 ]
  br i1 %86, label %481, label %594

481:                                              ; preds = %478
  %482 = sext i32 %480 to i64
  br i1 %475, label %483, label %486

483:                                              ; preds = %575, %481
  %484 = phi i64 [ %482, %481 ], [ %487, %575 ]
  %485 = phi i64 [ 0, %481 ], [ %476, %575 ]
  br label %598

486:                                              ; preds = %481
  %487 = add nsw i64 %476, %482
  br label %488

488:                                              ; preds = %488, %486
  %489 = phi i64 [ 0, %486 ], [ %573, %488 ]
  %490 = add i64 %489, %482
  %491 = or i64 %489, 1
  %492 = or i64 %489, 2
  %493 = or i64 %489, 3
  %494 = or i64 %489, 4
  %495 = or i64 %489, 5
  %496 = or i64 %489, 6
  %497 = or i64 %489, 7
  %498 = or i64 %489, 8
  %499 = or i64 %489, 9
  %500 = or i64 %489, 10
  %501 = or i64 %489, 11
  %502 = or i64 %489, 12
  %503 = or i64 %489, 13
  %504 = or i64 %489, 14
  %505 = or i64 %489, 15
  %506 = getelementptr inbounds i8, i8* %0, i64 %490
  %507 = bitcast i8* %506 to <16 x i8>*
  %508 = load <16 x i8>, <16 x i8>* %507, align 1, !tbaa !3
  %509 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %489
  %510 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %491
  %511 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %492
  %512 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %493
  %513 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %494
  %514 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %495
  %515 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %496
  %516 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %497
  %517 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %498
  %518 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %499
  %519 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %500
  %520 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %501
  %521 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %502
  %522 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %503
  %523 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %504
  %524 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %505
  %525 = bitcast <2 x i64>* %509 to i8*
  %526 = bitcast <2 x i64>* %510 to i8*
  %527 = bitcast <2 x i64>* %511 to i8*
  %528 = bitcast <2 x i64>* %512 to i8*
  %529 = bitcast <2 x i64>* %513 to i8*
  %530 = bitcast <2 x i64>* %514 to i8*
  %531 = bitcast <2 x i64>* %515 to i8*
  %532 = bitcast <2 x i64>* %516 to i8*
  %533 = bitcast <2 x i64>* %517 to i8*
  %534 = bitcast <2 x i64>* %518 to i8*
  %535 = bitcast <2 x i64>* %519 to i8*
  %536 = bitcast <2 x i64>* %520 to i8*
  %537 = bitcast <2 x i64>* %521 to i8*
  %538 = bitcast <2 x i64>* %522 to i8*
  %539 = bitcast <2 x i64>* %523 to i8*
  %540 = bitcast <2 x i64>* %524 to i8*
  %541 = getelementptr inbounds i8, i8* %525, i64 %479
  %542 = getelementptr inbounds i8, i8* %526, i64 %479
  %543 = getelementptr inbounds i8, i8* %527, i64 %479
  %544 = getelementptr inbounds i8, i8* %528, i64 %479
  %545 = getelementptr inbounds i8, i8* %529, i64 %479
  %546 = getelementptr inbounds i8, i8* %530, i64 %479
  %547 = getelementptr inbounds i8, i8* %531, i64 %479
  %548 = getelementptr inbounds i8, i8* %532, i64 %479
  %549 = getelementptr inbounds i8, i8* %533, i64 %479
  %550 = getelementptr inbounds i8, i8* %534, i64 %479
  %551 = getelementptr inbounds i8, i8* %535, i64 %479
  %552 = getelementptr inbounds i8, i8* %536, i64 %479
  %553 = getelementptr inbounds i8, i8* %537, i64 %479
  %554 = getelementptr inbounds i8, i8* %538, i64 %479
  %555 = getelementptr inbounds i8, i8* %539, i64 %479
  %556 = getelementptr inbounds i8, i8* %540, i64 %479
  %557 = extractelement <16 x i8> %508, i32 0
  store i8 %557, i8* %541, align 1, !tbaa !3
  %558 = extractelement <16 x i8> %508, i32 1
  store i8 %558, i8* %542, align 1, !tbaa !3
  %559 = extractelement <16 x i8> %508, i32 2
  store i8 %559, i8* %543, align 1, !tbaa !3
  %560 = extractelement <16 x i8> %508, i32 3
  store i8 %560, i8* %544, align 1, !tbaa !3
  %561 = extractelement <16 x i8> %508, i32 4
  store i8 %561, i8* %545, align 1, !tbaa !3
  %562 = extractelement <16 x i8> %508, i32 5
  store i8 %562, i8* %546, align 1, !tbaa !3
  %563 = extractelement <16 x i8> %508, i32 6
  store i8 %563, i8* %547, align 1, !tbaa !3
  %564 = extractelement <16 x i8> %508, i32 7
  store i8 %564, i8* %548, align 1, !tbaa !3
  %565 = extractelement <16 x i8> %508, i32 8
  store i8 %565, i8* %549, align 1, !tbaa !3
  %566 = extractelement <16 x i8> %508, i32 9
  store i8 %566, i8* %550, align 1, !tbaa !3
  %567 = extractelement <16 x i8> %508, i32 10
  store i8 %567, i8* %551, align 1, !tbaa !3
  %568 = extractelement <16 x i8> %508, i32 11
  store i8 %568, i8* %552, align 1, !tbaa !3
  %569 = extractelement <16 x i8> %508, i32 12
  store i8 %569, i8* %553, align 1, !tbaa !3
  %570 = extractelement <16 x i8> %508, i32 13
  store i8 %570, i8* %554, align 1, !tbaa !3
  %571 = extractelement <16 x i8> %508, i32 14
  store i8 %571, i8* %555, align 1, !tbaa !3
  %572 = extractelement <16 x i8> %508, i32 15
  store i8 %572, i8* %556, align 1, !tbaa !3
  %573 = add i64 %489, 16
  %574 = icmp eq i64 %573, %476
  br i1 %574, label %575, label %488, !llvm.loop !115

575:                                              ; preds = %488
  br i1 %477, label %591, label %483

576:                                              ; preds = %594, %470
  %577 = bitcast <2 x i64>* %8 to i8*
  %578 = mul nsw i32 %467, %2
  %579 = sext i32 %578 to i64
  %580 = getelementptr inbounds i8, i8* %0, i64 %579
  %581 = sext i32 %2 to i64
  %582 = zext i32 %4 to i64
  %583 = zext i32 %471 to i64
  %584 = add nsw i64 %583, -1
  %585 = add nsw i64 %583, -2
  %586 = icmp eq i32 %471, 1
  %587 = and i64 %584, 3
  %588 = icmp ult i64 %585, 3
  %589 = and i64 %584, -4
  %590 = icmp eq i64 %587, 0
  br label %609

591:                                              ; preds = %598, %575
  %592 = phi i64 [ %487, %575 ], [ %601, %598 ]
  %593 = trunc i64 %592 to i32
  br label %594

594:                                              ; preds = %591, %478
  %595 = phi i32 [ %480, %478 ], [ %593, %591 ]
  %596 = add nuw nsw i64 %479, 1
  %597 = icmp eq i64 %596, %474
  br i1 %597, label %576, label %478

598:                                              ; preds = %483, %598
  %599 = phi i64 [ %601, %598 ], [ %484, %483 ]
  %600 = phi i64 [ %607, %598 ], [ %485, %483 ]
  %601 = add nsw i64 %599, 1
  %602 = getelementptr inbounds i8, i8* %0, i64 %599
  %603 = load i8, i8* %602, align 1, !tbaa !3
  %604 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %600
  %605 = bitcast <2 x i64>* %604 to i8*
  %606 = getelementptr inbounds i8, i8* %605, i64 %479
  store i8 %603, i8* %606, align 1, !tbaa !3
  %607 = add nuw nsw i64 %600, 1
  %608 = icmp eq i64 %607, %187
  br i1 %608, label %591, label %598, !llvm.loop !116

609:                                              ; preds = %659, %576
  %610 = phi i64 [ 0, %576 ], [ %660, %659 ]
  %611 = phi [2 x <2 x i64>]* [ %135, %576 ], [ %661, %659 ]
  call void @llvm.lifetime.start.p0i8(i64 16, i8* nonnull %577) #2
  store <2 x i64> zeroinitializer, <2 x i64>* %8, align 16, !tbaa !3
  br i1 %86, label %623, label %615

612:                                              ; preds = %623
  %613 = bitcast <2 x i64> %643 to <16 x i8>
  %614 = extractelement <16 x i8> %613, i32 0
  br label %615

615:                                              ; preds = %612, %609
  %616 = phi i8 [ %614, %612 ], [ 0, %609 ]
  br i1 %472, label %617, label %659

617:                                              ; preds = %615
  %618 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %610, i64 0
  %619 = load i32, i32* %618, align 4, !tbaa !22
  %620 = sext i32 %619 to i64
  %621 = getelementptr inbounds i8, i8* %580, i64 %620
  store i8 %616, i8* %621, align 1, !tbaa !3
  br i1 %586, label %659, label %622

622:                                              ; preds = %617
  br i1 %588, label %646, label %663

623:                                              ; preds = %609, %623
  %624 = phi <2 x i64> [ %643, %623 ], [ zeroinitializer, %609 ]
  %625 = phi i64 [ %644, %623 ], [ 0, %609 ]
  %626 = getelementptr inbounds <2 x i64>, <2 x i64>* %188, i64 %625
  %627 = load <2 x i64>, <2 x i64>* %626, align 16, !tbaa !3
  %628 = getelementptr inbounds [2 x <2 x i64>], [2 x <2 x i64>]* %611, i64 %625, i64 0
  %629 = bitcast <2 x i64>* %628 to <16 x i8>*
  %630 = load <16 x i8>, <16 x i8>* %629, align 16, !tbaa !3
  %631 = bitcast <2 x i64> %627 to <16 x i8>
  %632 = and <16 x i8> %631, <i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15>
  %633 = call <16 x i8> @llvm.x86.ssse3.pshuf.b.128(<16 x i8> %630, <16 x i8> %632) #2
  %634 = lshr <2 x i64> %627, <i64 4, i64 4>
  %635 = getelementptr inbounds [2 x <2 x i64>], [2 x <2 x i64>]* %611, i64 %625, i64 1
  %636 = bitcast <2 x i64>* %635 to <16 x i8>*
  %637 = load <16 x i8>, <16 x i8>* %636, align 16, !tbaa !3
  %638 = bitcast <2 x i64> %634 to <16 x i8>
  %639 = and <16 x i8> %638, <i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15>
  %640 = call <16 x i8> @llvm.x86.ssse3.pshuf.b.128(<16 x i8> %637, <16 x i8> %639) #2
  %641 = xor <16 x i8> %640, %633
  %642 = bitcast <16 x i8> %641 to <2 x i64>
  %643 = xor <2 x i64> %624, %642
  store <2 x i64> %643, <2 x i64>* %8, align 16, !tbaa !3
  %644 = add nuw nsw i64 %625, 1
  %645 = icmp eq i64 %644, %187
  br i1 %645, label %612, label %623

646:                                              ; preds = %663, %622
  %647 = phi i64 [ 1, %622 ], [ %682, %663 ]
  %648 = phi i8* [ %621, %622 ], [ %679, %663 ]
  br i1 %590, label %659, label %649

649:                                              ; preds = %646, %649
  %650 = phi i64 [ %656, %649 ], [ %647, %646 ]
  %651 = phi i8* [ %653, %649 ], [ %648, %646 ]
  %652 = phi i64 [ %657, %649 ], [ %587, %646 ]
  %653 = getelementptr inbounds i8, i8* %651, i64 %581
  %654 = getelementptr inbounds i8, i8* %577, i64 %650
  %655 = load i8, i8* %654, align 1, !tbaa !3
  store i8 %655, i8* %653, align 1, !tbaa !3
  %656 = add nuw nsw i64 %650, 1
  %657 = add i64 %652, -1
  %658 = icmp eq i64 %657, 0
  br i1 %658, label %659, label %649, !llvm.loop !117

659:                                              ; preds = %646, %649, %617, %615
  call void @llvm.lifetime.end.p0i8(i64 16, i8* nonnull %577) #2
  %660 = add nuw nsw i64 %610, 1
  %661 = getelementptr inbounds [2 x <2 x i64>], [2 x <2 x i64>]* %611, i64 %581
  %662 = icmp eq i64 %660, %582
  br i1 %662, label %685, label %609

663:                                              ; preds = %622, %663
  %664 = phi i64 [ %682, %663 ], [ 1, %622 ]
  %665 = phi i8* [ %679, %663 ], [ %621, %622 ]
  %666 = phi i64 [ %683, %663 ], [ %589, %622 ]
  %667 = getelementptr inbounds i8, i8* %665, i64 %581
  %668 = getelementptr inbounds i8, i8* %577, i64 %664
  %669 = load i8, i8* %668, align 1, !tbaa !3
  store i8 %669, i8* %667, align 1, !tbaa !3
  %670 = add nuw nsw i64 %664, 1
  %671 = getelementptr inbounds i8, i8* %667, i64 %581
  %672 = getelementptr inbounds i8, i8* %577, i64 %670
  %673 = load i8, i8* %672, align 1, !tbaa !3
  store i8 %673, i8* %671, align 1, !tbaa !3
  %674 = add nuw nsw i64 %664, 2
  %675 = getelementptr inbounds i8, i8* %671, i64 %581
  %676 = getelementptr inbounds i8, i8* %577, i64 %674
  %677 = load i8, i8* %676, align 1, !tbaa !3
  store i8 %677, i8* %675, align 1, !tbaa !3
  %678 = add nuw nsw i64 %664, 3
  %679 = getelementptr inbounds i8, i8* %675, i64 %581
  %680 = getelementptr inbounds i8, i8* %577, i64 %678
  %681 = load i8, i8* %680, align 1, !tbaa !3
  store i8 %681, i8* %679, align 1, !tbaa !3
  %682 = add nuw nsw i64 %664, 4
  %683 = add i64 %666, -4
  %684 = icmp eq i64 %683, 0
  br i1 %684, label %646, label %663

685:                                              ; preds = %659, %466
  call void @llvm.stackrestore(i8* %83)
  br label %686

686:                                              ; preds = %22, %29, %79, %57, %14, %11, %5, %685
  %687 = phi i32 [ 0, %685 ], [ 1, %5 ], [ 2, %11 ], [ 3, %14 ], [ 4, %79 ], [ 4, %57 ], [ 4, %29 ], [ 4, %22 ]
  ret i32 %687
}

; Function Attrs: nounwind uwtable
define dso_local i32 @ec_encode_avx2(i8* noalias nocapture readonly %0, i32 %1, i8* noalias nocapture %2, i32 %3, i32 %4, i32 %5) local_unnamed_addr #5 {
  %7 = alloca <4 x i64>, align 32
  %8 = alloca <4 x i64>, align 32
  %9 = add i32 %4, -1
  %10 = icmp ugt i32 %9, 253
  br i1 %10, label %625, label %11

11:                                               ; preds = %6
  %12 = add i32 %5, -1
  %13 = icmp ugt i32 %12, 253
  br i1 %13, label %625, label %14

14:                                               ; preds = %11
  %15 = srem i32 %1, %4
  %16 = sdiv i32 %1, %4
  %17 = icmp eq i32 %15, 0
  br i1 %17, label %18, label %625

18:                                               ; preds = %14
  %19 = srem i32 %3, %5
  %20 = sdiv i32 %3, %5
  %21 = icmp eq i32 %19, 0
  br i1 %21, label %22, label %625

22:                                               ; preds = %18
  %23 = icmp eq i32 %16, %20
  br i1 %23, label %24, label %625

24:                                               ; preds = %22
  %25 = mul nuw nsw i32 %5, %4
  %26 = zext i32 %25 to i64
  %27 = tail call i8* @llvm.stacksave()
  %28 = alloca [2 x <4 x i64>], i64 %26, align 32
  br label %65

29:                                               ; preds = %77
  %30 = zext i32 %4 to i64
  %31 = alloca <4 x i64>, i64 %30, align 32
  %32 = add i32 %16, -32
  %33 = icmp slt i32 %16, 32
  br i1 %33, label %401, label %34

34:                                               ; preds = %29
  %35 = icmp sgt i32 %4, 0
  %36 = bitcast <4 x i64>* %7 to i8*
  %37 = sext i32 %5 to i64
  %38 = sext i32 %4 to i64
  %39 = shl i32 %5, 5
  %40 = sext i32 %39 to i64
  %41 = zext i32 %5 to i64
  %42 = getelementptr inbounds i8, i8* %36, i64 12
  %43 = getelementptr inbounds i8, i8* %36, i64 13
  %44 = getelementptr inbounds i8, i8* %36, i64 14
  %45 = getelementptr inbounds i8, i8* %36, i64 15
  %46 = getelementptr inbounds i8, i8* %36, i64 16
  %47 = getelementptr inbounds i8, i8* %36, i64 17
  %48 = getelementptr inbounds i8, i8* %36, i64 18
  %49 = getelementptr inbounds i8, i8* %36, i64 19
  %50 = getelementptr inbounds i8, i8* %36, i64 20
  %51 = getelementptr inbounds i8, i8* %36, i64 21
  %52 = getelementptr inbounds i8, i8* %36, i64 22
  %53 = getelementptr inbounds i8, i8* %36, i64 23
  %54 = getelementptr inbounds i8, i8* %36, i64 24
  %55 = getelementptr inbounds i8, i8* %36, i64 25
  %56 = getelementptr inbounds i8, i8* %36, i64 26
  %57 = getelementptr inbounds i8, i8* %36, i64 27
  %58 = getelementptr inbounds i8, i8* %36, i64 28
  %59 = getelementptr inbounds i8, i8* %36, i64 29
  %60 = getelementptr inbounds i8, i8* %36, i64 30
  %61 = getelementptr inbounds i8, i8* %36, i64 31
  %62 = icmp ult i32 %4, 16
  %63 = and i64 %30, 4294967280
  %64 = icmp eq i64 %63, %30
  br label %121

65:                                               ; preds = %77, %24
  %66 = phi i32 [ 0, %24 ], [ %78, %77 ]
  %67 = phi i64 [ 0, %24 ], [ %102, %77 ]
  %68 = urem i32 %66, 255
  %69 = add nuw nsw i32 %68, 1
  %70 = zext i32 %69 to i64
  %71 = getelementptr inbounds [256 x i8], [256 x i8]* @"?value_table@GF@@0QBEB", i64 0, i64 %70
  %72 = load i8, i8* %71, align 1, !tbaa !3, !noalias !118
  %73 = zext i8 %72 to i64
  %74 = getelementptr inbounds [256 x i8], [256 x i8]* @"?exp_table@GF@@0QBEB", i64 0, i64 %73
  %75 = shl i64 %67, 32
  %76 = ashr exact i64 %75, 32
  br label %80

77:                                               ; preds = %100
  %78 = add nuw nsw i32 %66, 1
  %79 = icmp eq i32 %78, %5
  br i1 %79, label %29, label %65

80:                                               ; preds = %100, %65
  %81 = phi i64 [ %76, %65 ], [ %102, %100 ]
  %82 = phi i32 [ 0, %65 ], [ %101, %100 ]
  %83 = icmp eq i32 %82, 0
  br i1 %83, label %93, label %84

84:                                               ; preds = %80
  %85 = load i8, i8* %74, align 1, !tbaa !3, !noalias !121
  %86 = zext i8 %85 to i32
  %87 = mul nsw i32 %82, %86
  %88 = urem i32 %87, 255
  %89 = add nuw nsw i32 %88, 1
  %90 = zext i32 %89 to i64
  %91 = getelementptr inbounds [256 x i8], [256 x i8]* @"?value_table@GF@@0QBEB", i64 0, i64 %90
  %92 = load i8, i8* %91, align 1, !tbaa !3, !noalias !124
  br label %93

93:                                               ; preds = %80, %84
  %94 = phi i8 [ %92, %84 ], [ 1, %80 ]
  %95 = zext i8 %94 to i64
  %96 = getelementptr inbounds [2 x <4 x i64>], [2 x <4 x i64>]* %28, i64 %81
  %97 = bitcast [2 x <4 x i64>]* %96 to i8*
  %98 = getelementptr inbounds [2 x <4 x i64>], [2 x <4 x i64>]* %28, i64 %81, i64 1
  %99 = bitcast <4 x i64>* %98 to i8*
  br label %104

100:                                              ; preds = %104
  %101 = add nuw nsw i32 %82, 1
  %102 = add nsw i64 %81, 1
  %103 = icmp eq i32 %101, %4
  br i1 %103, label %77, label %80

104:                                              ; preds = %104, %93
  %105 = phi i64 [ 0, %93 ], [ %119, %104 ]
  %106 = shl i64 %105, 8
  %107 = or i64 %106, %95
  %108 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %107, i32 0
  %109 = load i8, i8* %108, align 1, !tbaa !3, !noalias !127
  %110 = add nuw nsw i64 %105, 16
  %111 = getelementptr inbounds i8, i8* %97, i64 %110
  store i8 %109, i8* %111, align 1, !tbaa !3
  %112 = getelementptr inbounds i8, i8* %97, i64 %105
  store i8 %109, i8* %112, align 1, !tbaa !3
  %113 = shl nuw nsw i64 %105, 12
  %114 = or i64 %113, %95
  %115 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %114, i32 0
  %116 = load i8, i8* %115, align 1, !tbaa !3, !noalias !130
  %117 = getelementptr inbounds i8, i8* %99, i64 %110
  store i8 %116, i8* %117, align 1, !tbaa !3
  %118 = getelementptr inbounds i8, i8* %99, i64 %105
  store i8 %116, i8* %118, align 1, !tbaa !3
  %119 = add nuw nsw i64 %105, 1
  %120 = icmp eq i64 %119, 16
  br i1 %120, label %100, label %104

121:                                              ; preds = %34, %274
  %122 = phi i64 [ 0, %34 ], [ %276, %274 ]
  %123 = phi i32 [ 0, %34 ], [ %260, %274 ]
  %124 = phi i32 [ 0, %34 ], [ %275, %274 ]
  br label %158

125:                                              ; preds = %259
  %126 = getelementptr inbounds i8, i8* %2, i64 %122
  %127 = getelementptr inbounds i8, i8* %126, i64 %37
  %128 = getelementptr inbounds i8, i8* %127, i64 %37
  %129 = getelementptr inbounds i8, i8* %128, i64 %37
  %130 = getelementptr inbounds i8, i8* %129, i64 %37
  %131 = getelementptr inbounds i8, i8* %130, i64 %37
  %132 = getelementptr inbounds i8, i8* %131, i64 %37
  %133 = getelementptr inbounds i8, i8* %132, i64 %37
  %134 = getelementptr inbounds i8, i8* %133, i64 %37
  %135 = getelementptr inbounds i8, i8* %134, i64 %37
  %136 = getelementptr inbounds i8, i8* %135, i64 %37
  %137 = getelementptr inbounds i8, i8* %136, i64 %37
  %138 = getelementptr inbounds i8, i8* %137, i64 %37
  %139 = getelementptr inbounds i8, i8* %138, i64 %37
  %140 = getelementptr inbounds i8, i8* %139, i64 %37
  %141 = getelementptr inbounds i8, i8* %140, i64 %37
  %142 = getelementptr inbounds i8, i8* %141, i64 %37
  %143 = getelementptr inbounds i8, i8* %142, i64 %37
  %144 = getelementptr inbounds i8, i8* %143, i64 %37
  %145 = getelementptr inbounds i8, i8* %144, i64 %37
  %146 = getelementptr inbounds i8, i8* %145, i64 %37
  %147 = getelementptr inbounds i8, i8* %146, i64 %37
  %148 = getelementptr inbounds i8, i8* %147, i64 %37
  %149 = getelementptr inbounds i8, i8* %148, i64 %37
  %150 = getelementptr inbounds i8, i8* %149, i64 %37
  %151 = getelementptr inbounds i8, i8* %150, i64 %37
  %152 = getelementptr inbounds i8, i8* %151, i64 %37
  %153 = getelementptr inbounds i8, i8* %152, i64 %37
  %154 = getelementptr inbounds i8, i8* %153, i64 %37
  %155 = getelementptr inbounds i8, i8* %154, i64 %37
  %156 = getelementptr inbounds i8, i8* %155, i64 %37
  %157 = getelementptr inbounds i8, i8* %156, i64 %37
  br label %278

158:                                              ; preds = %259, %121
  %159 = phi i64 [ 0, %121 ], [ %261, %259 ]
  %160 = phi i32 [ %123, %121 ], [ %260, %259 ]
  br i1 %35, label %161, label %259

161:                                              ; preds = %158
  %162 = sext i32 %160 to i64
  br i1 %62, label %163, label %166

163:                                              ; preds = %255, %161
  %164 = phi i64 [ %162, %161 ], [ %167, %255 ]
  %165 = phi i64 [ 0, %161 ], [ %63, %255 ]
  br label %263

166:                                              ; preds = %161
  %167 = add nsw i64 %63, %162
  br label %168

168:                                              ; preds = %168, %166
  %169 = phi i64 [ 0, %166 ], [ %253, %168 ]
  %170 = add i64 %169, %162
  %171 = or i64 %169, 1
  %172 = or i64 %169, 2
  %173 = or i64 %169, 3
  %174 = or i64 %169, 4
  %175 = or i64 %169, 5
  %176 = or i64 %169, 6
  %177 = or i64 %169, 7
  %178 = or i64 %169, 8
  %179 = or i64 %169, 9
  %180 = or i64 %169, 10
  %181 = or i64 %169, 11
  %182 = or i64 %169, 12
  %183 = or i64 %169, 13
  %184 = or i64 %169, 14
  %185 = or i64 %169, 15
  %186 = getelementptr inbounds i8, i8* %0, i64 %170
  %187 = bitcast i8* %186 to <16 x i8>*
  %188 = load <16 x i8>, <16 x i8>* %187, align 1, !tbaa !3
  %189 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %169
  %190 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %171
  %191 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %172
  %192 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %173
  %193 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %174
  %194 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %175
  %195 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %176
  %196 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %177
  %197 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %178
  %198 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %179
  %199 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %180
  %200 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %181
  %201 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %182
  %202 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %183
  %203 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %184
  %204 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %185
  %205 = bitcast <4 x i64>* %189 to i8*
  %206 = bitcast <4 x i64>* %190 to i8*
  %207 = bitcast <4 x i64>* %191 to i8*
  %208 = bitcast <4 x i64>* %192 to i8*
  %209 = bitcast <4 x i64>* %193 to i8*
  %210 = bitcast <4 x i64>* %194 to i8*
  %211 = bitcast <4 x i64>* %195 to i8*
  %212 = bitcast <4 x i64>* %196 to i8*
  %213 = bitcast <4 x i64>* %197 to i8*
  %214 = bitcast <4 x i64>* %198 to i8*
  %215 = bitcast <4 x i64>* %199 to i8*
  %216 = bitcast <4 x i64>* %200 to i8*
  %217 = bitcast <4 x i64>* %201 to i8*
  %218 = bitcast <4 x i64>* %202 to i8*
  %219 = bitcast <4 x i64>* %203 to i8*
  %220 = bitcast <4 x i64>* %204 to i8*
  %221 = getelementptr inbounds i8, i8* %205, i64 %159
  %222 = getelementptr inbounds i8, i8* %206, i64 %159
  %223 = getelementptr inbounds i8, i8* %207, i64 %159
  %224 = getelementptr inbounds i8, i8* %208, i64 %159
  %225 = getelementptr inbounds i8, i8* %209, i64 %159
  %226 = getelementptr inbounds i8, i8* %210, i64 %159
  %227 = getelementptr inbounds i8, i8* %211, i64 %159
  %228 = getelementptr inbounds i8, i8* %212, i64 %159
  %229 = getelementptr inbounds i8, i8* %213, i64 %159
  %230 = getelementptr inbounds i8, i8* %214, i64 %159
  %231 = getelementptr inbounds i8, i8* %215, i64 %159
  %232 = getelementptr inbounds i8, i8* %216, i64 %159
  %233 = getelementptr inbounds i8, i8* %217, i64 %159
  %234 = getelementptr inbounds i8, i8* %218, i64 %159
  %235 = getelementptr inbounds i8, i8* %219, i64 %159
  %236 = getelementptr inbounds i8, i8* %220, i64 %159
  %237 = extractelement <16 x i8> %188, i32 0
  store i8 %237, i8* %221, align 1, !tbaa !3
  %238 = extractelement <16 x i8> %188, i32 1
  store i8 %238, i8* %222, align 1, !tbaa !3
  %239 = extractelement <16 x i8> %188, i32 2
  store i8 %239, i8* %223, align 1, !tbaa !3
  %240 = extractelement <16 x i8> %188, i32 3
  store i8 %240, i8* %224, align 1, !tbaa !3
  %241 = extractelement <16 x i8> %188, i32 4
  store i8 %241, i8* %225, align 1, !tbaa !3
  %242 = extractelement <16 x i8> %188, i32 5
  store i8 %242, i8* %226, align 1, !tbaa !3
  %243 = extractelement <16 x i8> %188, i32 6
  store i8 %243, i8* %227, align 1, !tbaa !3
  %244 = extractelement <16 x i8> %188, i32 7
  store i8 %244, i8* %228, align 1, !tbaa !3
  %245 = extractelement <16 x i8> %188, i32 8
  store i8 %245, i8* %229, align 1, !tbaa !3
  %246 = extractelement <16 x i8> %188, i32 9
  store i8 %246, i8* %230, align 1, !tbaa !3
  %247 = extractelement <16 x i8> %188, i32 10
  store i8 %247, i8* %231, align 1, !tbaa !3
  %248 = extractelement <16 x i8> %188, i32 11
  store i8 %248, i8* %232, align 1, !tbaa !3
  %249 = extractelement <16 x i8> %188, i32 12
  store i8 %249, i8* %233, align 1, !tbaa !3
  %250 = extractelement <16 x i8> %188, i32 13
  store i8 %250, i8* %234, align 1, !tbaa !3
  %251 = extractelement <16 x i8> %188, i32 14
  store i8 %251, i8* %235, align 1, !tbaa !3
  %252 = extractelement <16 x i8> %188, i32 15
  store i8 %252, i8* %236, align 1, !tbaa !3
  %253 = add i64 %169, 16
  %254 = icmp eq i64 %253, %63
  br i1 %254, label %255, label %168, !llvm.loop !133

255:                                              ; preds = %168
  br i1 %64, label %256, label %163

256:                                              ; preds = %263, %255
  %257 = phi i64 [ %167, %255 ], [ %266, %263 ]
  %258 = trunc i64 %257 to i32
  br label %259

259:                                              ; preds = %256, %158
  %260 = phi i32 [ %160, %158 ], [ %258, %256 ]
  %261 = add nuw nsw i64 %159, 1
  %262 = icmp eq i64 %261, 32
  br i1 %262, label %125, label %158

263:                                              ; preds = %163, %263
  %264 = phi i64 [ %266, %263 ], [ %164, %163 ]
  %265 = phi i64 [ %272, %263 ], [ %165, %163 ]
  %266 = add nsw i64 %264, 1
  %267 = getelementptr inbounds i8, i8* %0, i64 %264
  %268 = load i8, i8* %267, align 1, !tbaa !3
  %269 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %265
  %270 = bitcast <4 x i64>* %269 to i8*
  %271 = getelementptr inbounds i8, i8* %270, i64 %159
  store i8 %268, i8* %271, align 1, !tbaa !3
  %272 = add nuw nsw i64 %265, 1
  %273 = icmp eq i64 %272, %30
  br i1 %273, label %256, label %263, !llvm.loop !134

274:                                              ; preds = %306
  %275 = add nuw nsw i32 %124, 32
  %276 = add nsw i64 %122, %40
  %277 = icmp sgt i32 %275, %32
  br i1 %277, label %397, label %121

278:                                              ; preds = %306, %125
  %279 = phi i64 [ 0, %125 ], [ %371, %306 ]
  %280 = phi [2 x <4 x i64>]* [ %28, %125 ], [ %372, %306 ]
  call void @llvm.lifetime.start.p0i8(i64 32, i8* nonnull %36) #2
  store <4 x i64> zeroinitializer, <4 x i64>* %7, align 32, !tbaa !3
  br i1 %35, label %374, label %306

281:                                              ; preds = %374
  %282 = bitcast <4 x i64> %394 to <32 x i8>
  %283 = extractelement <32 x i8> %282, i32 11
  %284 = bitcast <4 x i64> %394 to <32 x i8>
  %285 = extractelement <32 x i8> %284, i32 10
  %286 = bitcast <4 x i64> %394 to <32 x i8>
  %287 = extractelement <32 x i8> %286, i32 9
  %288 = bitcast <4 x i64> %394 to <32 x i8>
  %289 = extractelement <32 x i8> %288, i32 8
  %290 = bitcast <4 x i64> %394 to <32 x i8>
  %291 = extractelement <32 x i8> %290, i32 7
  %292 = bitcast <4 x i64> %394 to <32 x i8>
  %293 = extractelement <32 x i8> %292, i32 6
  %294 = bitcast <4 x i64> %394 to <32 x i8>
  %295 = extractelement <32 x i8> %294, i32 5
  %296 = bitcast <4 x i64> %394 to <32 x i8>
  %297 = extractelement <32 x i8> %296, i32 4
  %298 = bitcast <4 x i64> %394 to <32 x i8>
  %299 = extractelement <32 x i8> %298, i32 3
  %300 = bitcast <4 x i64> %394 to <32 x i8>
  %301 = extractelement <32 x i8> %300, i32 2
  %302 = bitcast <4 x i64> %394 to <32 x i8>
  %303 = extractelement <32 x i8> %302, i32 1
  %304 = bitcast <4 x i64> %394 to <32 x i8>
  %305 = extractelement <32 x i8> %304, i32 0
  br label %306

306:                                              ; preds = %281, %278
  %307 = phi i8 [ %283, %281 ], [ 0, %278 ]
  %308 = phi i8 [ %285, %281 ], [ 0, %278 ]
  %309 = phi i8 [ %287, %281 ], [ 0, %278 ]
  %310 = phi i8 [ %289, %281 ], [ 0, %278 ]
  %311 = phi i8 [ %291, %281 ], [ 0, %278 ]
  %312 = phi i8 [ %293, %281 ], [ 0, %278 ]
  %313 = phi i8 [ %295, %281 ], [ 0, %278 ]
  %314 = phi i8 [ %297, %281 ], [ 0, %278 ]
  %315 = phi i8 [ %299, %281 ], [ 0, %278 ]
  %316 = phi i8 [ %301, %281 ], [ 0, %278 ]
  %317 = phi i8 [ %303, %281 ], [ 0, %278 ]
  %318 = phi i8 [ %305, %281 ], [ 0, %278 ]
  %319 = getelementptr inbounds i8, i8* %126, i64 %279
  store i8 %318, i8* %319, align 1, !tbaa !3
  %320 = getelementptr inbounds i8, i8* %127, i64 %279
  store i8 %317, i8* %320, align 1, !tbaa !3
  %321 = getelementptr inbounds i8, i8* %128, i64 %279
  store i8 %316, i8* %321, align 1, !tbaa !3
  %322 = getelementptr inbounds i8, i8* %129, i64 %279
  store i8 %315, i8* %322, align 1, !tbaa !3
  %323 = getelementptr inbounds i8, i8* %130, i64 %279
  store i8 %314, i8* %323, align 1, !tbaa !3
  %324 = getelementptr inbounds i8, i8* %131, i64 %279
  store i8 %313, i8* %324, align 1, !tbaa !3
  %325 = getelementptr inbounds i8, i8* %132, i64 %279
  store i8 %312, i8* %325, align 1, !tbaa !3
  %326 = getelementptr inbounds i8, i8* %133, i64 %279
  store i8 %311, i8* %326, align 1, !tbaa !3
  %327 = getelementptr inbounds i8, i8* %134, i64 %279
  store i8 %310, i8* %327, align 1, !tbaa !3
  %328 = getelementptr inbounds i8, i8* %135, i64 %279
  store i8 %309, i8* %328, align 1, !tbaa !3
  %329 = getelementptr inbounds i8, i8* %136, i64 %279
  store i8 %308, i8* %329, align 1, !tbaa !3
  %330 = getelementptr inbounds i8, i8* %137, i64 %279
  store i8 %307, i8* %330, align 1, !tbaa !3
  %331 = load i8, i8* %42, align 4, !tbaa !3
  %332 = getelementptr inbounds i8, i8* %138, i64 %279
  store i8 %331, i8* %332, align 1, !tbaa !3
  %333 = load i8, i8* %43, align 1, !tbaa !3
  %334 = getelementptr inbounds i8, i8* %139, i64 %279
  store i8 %333, i8* %334, align 1, !tbaa !3
  %335 = load i8, i8* %44, align 2, !tbaa !3
  %336 = getelementptr inbounds i8, i8* %140, i64 %279
  store i8 %335, i8* %336, align 1, !tbaa !3
  %337 = load i8, i8* %45, align 1, !tbaa !3
  %338 = getelementptr inbounds i8, i8* %141, i64 %279
  store i8 %337, i8* %338, align 1, !tbaa !3
  %339 = load i8, i8* %46, align 16, !tbaa !3
  %340 = getelementptr inbounds i8, i8* %142, i64 %279
  store i8 %339, i8* %340, align 1, !tbaa !3
  %341 = load i8, i8* %47, align 1, !tbaa !3
  %342 = getelementptr inbounds i8, i8* %143, i64 %279
  store i8 %341, i8* %342, align 1, !tbaa !3
  %343 = load i8, i8* %48, align 2, !tbaa !3
  %344 = getelementptr inbounds i8, i8* %144, i64 %279
  store i8 %343, i8* %344, align 1, !tbaa !3
  %345 = load i8, i8* %49, align 1, !tbaa !3
  %346 = getelementptr inbounds i8, i8* %145, i64 %279
  store i8 %345, i8* %346, align 1, !tbaa !3
  %347 = load i8, i8* %50, align 4, !tbaa !3
  %348 = getelementptr inbounds i8, i8* %146, i64 %279
  store i8 %347, i8* %348, align 1, !tbaa !3
  %349 = load i8, i8* %51, align 1, !tbaa !3
  %350 = getelementptr inbounds i8, i8* %147, i64 %279
  store i8 %349, i8* %350, align 1, !tbaa !3
  %351 = load i8, i8* %52, align 2, !tbaa !3
  %352 = getelementptr inbounds i8, i8* %148, i64 %279
  store i8 %351, i8* %352, align 1, !tbaa !3
  %353 = load i8, i8* %53, align 1, !tbaa !3
  %354 = getelementptr inbounds i8, i8* %149, i64 %279
  store i8 %353, i8* %354, align 1, !tbaa !3
  %355 = load i8, i8* %54, align 8, !tbaa !3
  %356 = getelementptr inbounds i8, i8* %150, i64 %279
  store i8 %355, i8* %356, align 1, !tbaa !3
  %357 = load i8, i8* %55, align 1, !tbaa !3
  %358 = getelementptr inbounds i8, i8* %151, i64 %279
  store i8 %357, i8* %358, align 1, !tbaa !3
  %359 = load i8, i8* %56, align 2, !tbaa !3
  %360 = getelementptr inbounds i8, i8* %152, i64 %279
  store i8 %359, i8* %360, align 1, !tbaa !3
  %361 = load i8, i8* %57, align 1, !tbaa !3
  %362 = getelementptr inbounds i8, i8* %153, i64 %279
  store i8 %361, i8* %362, align 1, !tbaa !3
  %363 = load i8, i8* %58, align 4, !tbaa !3
  %364 = getelementptr inbounds i8, i8* %154, i64 %279
  store i8 %363, i8* %364, align 1, !tbaa !3
  %365 = load i8, i8* %59, align 1, !tbaa !3
  %366 = getelementptr inbounds i8, i8* %155, i64 %279
  store i8 %365, i8* %366, align 1, !tbaa !3
  %367 = load i8, i8* %60, align 2, !tbaa !3
  %368 = getelementptr inbounds i8, i8* %156, i64 %279
  store i8 %367, i8* %368, align 1, !tbaa !3
  %369 = load i8, i8* %61, align 1, !tbaa !3
  %370 = getelementptr inbounds i8, i8* %157, i64 %279
  store i8 %369, i8* %370, align 1, !tbaa !3
  call void @llvm.lifetime.end.p0i8(i64 32, i8* nonnull %36) #2
  %371 = add nuw nsw i64 %279, 1
  %372 = getelementptr inbounds [2 x <4 x i64>], [2 x <4 x i64>]* %280, i64 %38
  %373 = icmp eq i64 %371, %41
  br i1 %373, label %274, label %278

374:                                              ; preds = %278, %374
  %375 = phi <4 x i64> [ %394, %374 ], [ zeroinitializer, %278 ]
  %376 = phi i64 [ %395, %374 ], [ 0, %278 ]
  %377 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %376
  %378 = load <4 x i64>, <4 x i64>* %377, align 32, !tbaa !3
  %379 = getelementptr inbounds [2 x <4 x i64>], [2 x <4 x i64>]* %280, i64 %376, i64 0
  %380 = bitcast <4 x i64>* %379 to <32 x i8>*
  %381 = load <32 x i8>, <32 x i8>* %380, align 32, !tbaa !3
  %382 = bitcast <4 x i64> %378 to <32 x i8>
  %383 = and <32 x i8> %382, <i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15>
  %384 = tail call <32 x i8> @llvm.x86.avx2.pshuf.b(<32 x i8> %381, <32 x i8> %383) #2
  %385 = lshr <4 x i64> %378, <i64 4, i64 4, i64 4, i64 4>
  %386 = getelementptr inbounds [2 x <4 x i64>], [2 x <4 x i64>]* %280, i64 %376, i64 1
  %387 = bitcast <4 x i64>* %386 to <32 x i8>*
  %388 = load <32 x i8>, <32 x i8>* %387, align 32, !tbaa !3
  %389 = bitcast <4 x i64> %385 to <32 x i8>
  %390 = and <32 x i8> %389, <i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15>
  %391 = tail call <32 x i8> @llvm.x86.avx2.pshuf.b(<32 x i8> %388, <32 x i8> %390) #2
  %392 = xor <32 x i8> %391, %384
  %393 = bitcast <32 x i8> %392 to <4 x i64>
  %394 = xor <4 x i64> %375, %393
  store <4 x i64> %394, <4 x i64>* %7, align 32, !tbaa !3
  %395 = add nuw nsw i64 %376, 1
  %396 = icmp eq i64 %395, %30
  br i1 %396, label %281, label %374

397:                                              ; preds = %274
  %398 = and i32 %16, -32
  %399 = shl i64 %276, 32
  %400 = ashr exact i64 %399, 32
  br label %401

401:                                              ; preds = %397, %29
  %402 = phi i32 [ 0, %29 ], [ %398, %397 ]
  %403 = phi i32 [ 0, %29 ], [ %260, %397 ]
  %404 = phi i64 [ 0, %29 ], [ %400, %397 ]
  %405 = icmp sgt i32 %16, %402
  br i1 %405, label %406, label %624

406:                                              ; preds = %401
  %407 = sub i32 %16, %402
  %408 = icmp sgt i32 %407, 0
  br i1 %408, label %409, label %513

409:                                              ; preds = %406
  %410 = icmp sgt i32 %4, 0
  %411 = zext i32 %407 to i64
  %412 = icmp ult i32 %4, 16
  %413 = and i64 %30, 4294967280
  %414 = icmp eq i64 %413, %30
  br label %415

415:                                              ; preds = %531, %409
  %416 = phi i64 [ 0, %409 ], [ %533, %531 ]
  %417 = phi i32 [ %403, %409 ], [ %532, %531 ]
  br i1 %410, label %418, label %531

418:                                              ; preds = %415
  %419 = sext i32 %417 to i64
  br i1 %412, label %420, label %423

420:                                              ; preds = %512, %418
  %421 = phi i64 [ %419, %418 ], [ %424, %512 ]
  %422 = phi i64 [ 0, %418 ], [ %413, %512 ]
  br label %535

423:                                              ; preds = %418
  %424 = add nsw i64 %413, %419
  br label %425

425:                                              ; preds = %425, %423
  %426 = phi i64 [ 0, %423 ], [ %510, %425 ]
  %427 = add i64 %426, %419
  %428 = or i64 %426, 1
  %429 = or i64 %426, 2
  %430 = or i64 %426, 3
  %431 = or i64 %426, 4
  %432 = or i64 %426, 5
  %433 = or i64 %426, 6
  %434 = or i64 %426, 7
  %435 = or i64 %426, 8
  %436 = or i64 %426, 9
  %437 = or i64 %426, 10
  %438 = or i64 %426, 11
  %439 = or i64 %426, 12
  %440 = or i64 %426, 13
  %441 = or i64 %426, 14
  %442 = or i64 %426, 15
  %443 = getelementptr inbounds i8, i8* %0, i64 %427
  %444 = bitcast i8* %443 to <16 x i8>*
  %445 = load <16 x i8>, <16 x i8>* %444, align 1, !tbaa !3
  %446 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %426
  %447 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %428
  %448 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %429
  %449 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %430
  %450 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %431
  %451 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %432
  %452 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %433
  %453 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %434
  %454 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %435
  %455 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %436
  %456 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %437
  %457 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %438
  %458 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %439
  %459 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %440
  %460 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %441
  %461 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %442
  %462 = bitcast <4 x i64>* %446 to i8*
  %463 = bitcast <4 x i64>* %447 to i8*
  %464 = bitcast <4 x i64>* %448 to i8*
  %465 = bitcast <4 x i64>* %449 to i8*
  %466 = bitcast <4 x i64>* %450 to i8*
  %467 = bitcast <4 x i64>* %451 to i8*
  %468 = bitcast <4 x i64>* %452 to i8*
  %469 = bitcast <4 x i64>* %453 to i8*
  %470 = bitcast <4 x i64>* %454 to i8*
  %471 = bitcast <4 x i64>* %455 to i8*
  %472 = bitcast <4 x i64>* %456 to i8*
  %473 = bitcast <4 x i64>* %457 to i8*
  %474 = bitcast <4 x i64>* %458 to i8*
  %475 = bitcast <4 x i64>* %459 to i8*
  %476 = bitcast <4 x i64>* %460 to i8*
  %477 = bitcast <4 x i64>* %461 to i8*
  %478 = getelementptr inbounds i8, i8* %462, i64 %416
  %479 = getelementptr inbounds i8, i8* %463, i64 %416
  %480 = getelementptr inbounds i8, i8* %464, i64 %416
  %481 = getelementptr inbounds i8, i8* %465, i64 %416
  %482 = getelementptr inbounds i8, i8* %466, i64 %416
  %483 = getelementptr inbounds i8, i8* %467, i64 %416
  %484 = getelementptr inbounds i8, i8* %468, i64 %416
  %485 = getelementptr inbounds i8, i8* %469, i64 %416
  %486 = getelementptr inbounds i8, i8* %470, i64 %416
  %487 = getelementptr inbounds i8, i8* %471, i64 %416
  %488 = getelementptr inbounds i8, i8* %472, i64 %416
  %489 = getelementptr inbounds i8, i8* %473, i64 %416
  %490 = getelementptr inbounds i8, i8* %474, i64 %416
  %491 = getelementptr inbounds i8, i8* %475, i64 %416
  %492 = getelementptr inbounds i8, i8* %476, i64 %416
  %493 = getelementptr inbounds i8, i8* %477, i64 %416
  %494 = extractelement <16 x i8> %445, i32 0
  store i8 %494, i8* %478, align 1, !tbaa !3
  %495 = extractelement <16 x i8> %445, i32 1
  store i8 %495, i8* %479, align 1, !tbaa !3
  %496 = extractelement <16 x i8> %445, i32 2
  store i8 %496, i8* %480, align 1, !tbaa !3
  %497 = extractelement <16 x i8> %445, i32 3
  store i8 %497, i8* %481, align 1, !tbaa !3
  %498 = extractelement <16 x i8> %445, i32 4
  store i8 %498, i8* %482, align 1, !tbaa !3
  %499 = extractelement <16 x i8> %445, i32 5
  store i8 %499, i8* %483, align 1, !tbaa !3
  %500 = extractelement <16 x i8> %445, i32 6
  store i8 %500, i8* %484, align 1, !tbaa !3
  %501 = extractelement <16 x i8> %445, i32 7
  store i8 %501, i8* %485, align 1, !tbaa !3
  %502 = extractelement <16 x i8> %445, i32 8
  store i8 %502, i8* %486, align 1, !tbaa !3
  %503 = extractelement <16 x i8> %445, i32 9
  store i8 %503, i8* %487, align 1, !tbaa !3
  %504 = extractelement <16 x i8> %445, i32 10
  store i8 %504, i8* %488, align 1, !tbaa !3
  %505 = extractelement <16 x i8> %445, i32 11
  store i8 %505, i8* %489, align 1, !tbaa !3
  %506 = extractelement <16 x i8> %445, i32 12
  store i8 %506, i8* %490, align 1, !tbaa !3
  %507 = extractelement <16 x i8> %445, i32 13
  store i8 %507, i8* %491, align 1, !tbaa !3
  %508 = extractelement <16 x i8> %445, i32 14
  store i8 %508, i8* %492, align 1, !tbaa !3
  %509 = extractelement <16 x i8> %445, i32 15
  store i8 %509, i8* %493, align 1, !tbaa !3
  %510 = add i64 %426, 16
  %511 = icmp eq i64 %510, %413
  br i1 %511, label %512, label %425, !llvm.loop !135

512:                                              ; preds = %425
  br i1 %414, label %528, label %420

513:                                              ; preds = %531, %406
  %514 = bitcast <4 x i64>* %8 to i8*
  %515 = icmp sgt i32 %4, 0
  %516 = getelementptr inbounds i8, i8* %2, i64 %404
  %517 = sext i32 %5 to i64
  %518 = sext i32 %4 to i64
  %519 = zext i32 %5 to i64
  %520 = zext i32 %407 to i64
  %521 = add nsw i64 %520, -1
  %522 = add nsw i64 %520, -2
  %523 = icmp eq i32 %407, 1
  %524 = and i64 %521, 3
  %525 = icmp ult i64 %522, 3
  %526 = and i64 %521, -4
  %527 = icmp eq i64 %524, 0
  br label %546

528:                                              ; preds = %535, %512
  %529 = phi i64 [ %424, %512 ], [ %538, %535 ]
  %530 = trunc i64 %529 to i32
  br label %531

531:                                              ; preds = %528, %415
  %532 = phi i32 [ %417, %415 ], [ %530, %528 ]
  %533 = add nuw nsw i64 %416, 1
  %534 = icmp eq i64 %533, %411
  br i1 %534, label %513, label %415

535:                                              ; preds = %420, %535
  %536 = phi i64 [ %538, %535 ], [ %421, %420 ]
  %537 = phi i64 [ %544, %535 ], [ %422, %420 ]
  %538 = add nsw i64 %536, 1
  %539 = getelementptr inbounds i8, i8* %0, i64 %536
  %540 = load i8, i8* %539, align 1, !tbaa !3
  %541 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %537
  %542 = bitcast <4 x i64>* %541 to i8*
  %543 = getelementptr inbounds i8, i8* %542, i64 %416
  store i8 %540, i8* %543, align 1, !tbaa !3
  %544 = add nuw nsw i64 %537, 1
  %545 = icmp eq i64 %544, %30
  br i1 %545, label %528, label %535, !llvm.loop !136

546:                                              ; preds = %594, %513
  %547 = phi i64 [ 0, %513 ], [ %595, %594 ]
  %548 = phi [2 x <4 x i64>]* [ %28, %513 ], [ %596, %594 ]
  call void @llvm.lifetime.start.p0i8(i64 32, i8* nonnull %514) #2
  store <4 x i64> zeroinitializer, <4 x i64>* %8, align 32, !tbaa !3
  br i1 %515, label %557, label %552

549:                                              ; preds = %557
  %550 = bitcast <4 x i64> %577 to <32 x i8>
  %551 = extractelement <32 x i8> %550, i32 0
  br label %552

552:                                              ; preds = %549, %546
  %553 = phi i8 [ %551, %549 ], [ 0, %546 ]
  br i1 %408, label %554, label %594

554:                                              ; preds = %552
  %555 = getelementptr inbounds i8, i8* %516, i64 %547
  store i8 %553, i8* %555, align 1, !tbaa !3
  br i1 %523, label %594, label %556

556:                                              ; preds = %554
  br i1 %525, label %580, label %598

557:                                              ; preds = %546, %557
  %558 = phi <4 x i64> [ %577, %557 ], [ zeroinitializer, %546 ]
  %559 = phi i64 [ %578, %557 ], [ 0, %546 ]
  %560 = getelementptr inbounds <4 x i64>, <4 x i64>* %31, i64 %559
  %561 = load <4 x i64>, <4 x i64>* %560, align 32, !tbaa !3
  %562 = getelementptr inbounds [2 x <4 x i64>], [2 x <4 x i64>]* %548, i64 %559, i64 0
  %563 = bitcast <4 x i64>* %562 to <32 x i8>*
  %564 = load <32 x i8>, <32 x i8>* %563, align 32, !tbaa !3
  %565 = bitcast <4 x i64> %561 to <32 x i8>
  %566 = and <32 x i8> %565, <i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15>
  %567 = tail call <32 x i8> @llvm.x86.avx2.pshuf.b(<32 x i8> %564, <32 x i8> %566) #2
  %568 = lshr <4 x i64> %561, <i64 4, i64 4, i64 4, i64 4>
  %569 = getelementptr inbounds [2 x <4 x i64>], [2 x <4 x i64>]* %548, i64 %559, i64 1
  %570 = bitcast <4 x i64>* %569 to <32 x i8>*
  %571 = load <32 x i8>, <32 x i8>* %570, align 32, !tbaa !3
  %572 = bitcast <4 x i64> %568 to <32 x i8>
  %573 = and <32 x i8> %572, <i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15>
  %574 = tail call <32 x i8> @llvm.x86.avx2.pshuf.b(<32 x i8> %571, <32 x i8> %573) #2
  %575 = xor <32 x i8> %574, %567
  %576 = bitcast <32 x i8> %575 to <4 x i64>
  %577 = xor <4 x i64> %558, %576
  store <4 x i64> %577, <4 x i64>* %8, align 32, !tbaa !3
  %578 = add nuw nsw i64 %559, 1
  %579 = icmp eq i64 %578, %30
  br i1 %579, label %549, label %557

580:                                              ; preds = %598, %556
  %581 = phi i64 [ 1, %556 ], [ %621, %598 ]
  %582 = phi i8* [ %516, %556 ], [ %617, %598 ]
  br i1 %527, label %594, label %583

583:                                              ; preds = %580, %583
  %584 = phi i64 [ %591, %583 ], [ %581, %580 ]
  %585 = phi i8* [ %587, %583 ], [ %582, %580 ]
  %586 = phi i64 [ %592, %583 ], [ %524, %580 ]
  %587 = getelementptr inbounds i8, i8* %585, i64 %517
  %588 = getelementptr inbounds i8, i8* %514, i64 %584
  %589 = load i8, i8* %588, align 1, !tbaa !3
  %590 = getelementptr inbounds i8, i8* %587, i64 %547
  store i8 %589, i8* %590, align 1, !tbaa !3
  %591 = add nuw nsw i64 %584, 1
  %592 = add i64 %586, -1
  %593 = icmp eq i64 %592, 0
  br i1 %593, label %594, label %583, !llvm.loop !137

594:                                              ; preds = %580, %583, %554, %552
  call void @llvm.lifetime.end.p0i8(i64 32, i8* nonnull %514) #2
  %595 = add nuw nsw i64 %547, 1
  %596 = getelementptr inbounds [2 x <4 x i64>], [2 x <4 x i64>]* %548, i64 %518
  %597 = icmp eq i64 %595, %519
  br i1 %597, label %624, label %546

598:                                              ; preds = %556, %598
  %599 = phi i64 [ %621, %598 ], [ 1, %556 ]
  %600 = phi i8* [ %617, %598 ], [ %516, %556 ]
  %601 = phi i64 [ %622, %598 ], [ %526, %556 ]
  %602 = getelementptr inbounds i8, i8* %600, i64 %517
  %603 = getelementptr inbounds i8, i8* %514, i64 %599
  %604 = load i8, i8* %603, align 1, !tbaa !3
  %605 = getelementptr inbounds i8, i8* %602, i64 %547
  store i8 %604, i8* %605, align 1, !tbaa !3
  %606 = add nuw nsw i64 %599, 1
  %607 = getelementptr inbounds i8, i8* %602, i64 %517
  %608 = getelementptr inbounds i8, i8* %514, i64 %606
  %609 = load i8, i8* %608, align 1, !tbaa !3
  %610 = getelementptr inbounds i8, i8* %607, i64 %547
  store i8 %609, i8* %610, align 1, !tbaa !3
  %611 = add nuw nsw i64 %599, 2
  %612 = getelementptr inbounds i8, i8* %607, i64 %517
  %613 = getelementptr inbounds i8, i8* %514, i64 %611
  %614 = load i8, i8* %613, align 1, !tbaa !3
  %615 = getelementptr inbounds i8, i8* %612, i64 %547
  store i8 %614, i8* %615, align 1, !tbaa !3
  %616 = add nuw nsw i64 %599, 3
  %617 = getelementptr inbounds i8, i8* %612, i64 %517
  %618 = getelementptr inbounds i8, i8* %514, i64 %616
  %619 = load i8, i8* %618, align 1, !tbaa !3
  %620 = getelementptr inbounds i8, i8* %617, i64 %547
  store i8 %619, i8* %620, align 1, !tbaa !3
  %621 = add nuw nsw i64 %599, 4
  %622 = add i64 %601, -4
  %623 = icmp eq i64 %622, 0
  br i1 %623, label %580, label %598

624:                                              ; preds = %594, %401
  tail call void @llvm.stackrestore(i8* %27)
  br label %625

625:                                              ; preds = %624, %22, %18, %14, %11, %6
  %626 = phi i32 [ 1, %6 ], [ 2, %11 ], [ 3, %14 ], [ 4, %18 ], [ 0, %624 ], [ 5, %22 ]
  ret i32 %626
}

; Function Attrs: nounwind uwtable
define dso_local i32 @ec_decode_avx2(i8* noalias nocapture %0, i32 %1, i32 %2, [2 x i32]* noalias nocapture readonly %3, i32 %4) local_unnamed_addr #5 personality i32 (...)* @__CxxFrameHandler3 {
  %6 = alloca [32 x i8], align 16
  %7 = alloca <4 x i64>, align 32
  %8 = alloca <4 x i64>, align 32
  %9 = add i32 %2, -1
  %10 = icmp ugt i32 %9, 253
  br i1 %10, label %726, label %11

11:                                               ; preds = %5
  %12 = add i32 %4, -1
  %13 = icmp ugt i32 %12, 253
  br i1 %13, label %726, label %14

14:                                               ; preds = %11
  %15 = srem i32 %1, %2
  %16 = sdiv i32 %1, %2
  %17 = icmp eq i32 %15, 0
  br i1 %17, label %18, label %726

18:                                               ; preds = %14
  %19 = zext i32 %4 to i64
  br label %22

20:                                               ; preds = %29
  %21 = icmp eq i64 %33, %19
  br i1 %21, label %34, label %22

22:                                               ; preds = %20, %18
  %23 = phi i64 [ 0, %18 ], [ %33, %20 ]
  %24 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %23, i64 0
  %25 = load i32, i32* %24, align 4, !tbaa !22
  %26 = icmp sgt i32 %25, -1
  %27 = icmp slt i32 %25, %2
  %28 = and i1 %26, %27
  br i1 %28, label %29, label %726

29:                                               ; preds = %22
  %30 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %23, i64 1
  %31 = load i32, i32* %30, align 4, !tbaa !22
  %32 = icmp ugt i32 %31, 253
  %33 = add nuw nsw i64 %23, 1
  br i1 %32, label %726, label %20

34:                                               ; preds = %20
  %35 = getelementptr inbounds [32 x i8], [32 x i8]* %6, i64 0, i64 0
  call void @llvm.lifetime.start.p0i8(i64 32, i8* nonnull %35) #2
  call void @llvm.memset.p0i8.i64(i8* nonnull align 16 dereferenceable(32) %35, i8 0, i64 32, i1 false) #2
  %36 = zext i32 %4 to i64
  br label %37

37:                                               ; preds = %50, %34
  %38 = phi i64 [ 0, %34 ], [ %55, %50 ]
  %39 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %38, i64 0
  %40 = load i32, i32* %39, align 4, !tbaa !22
  %41 = sext i32 %40 to i64
  %42 = lshr i64 %41, 3
  %43 = getelementptr inbounds [32 x i8], [32 x i8]* %6, i64 0, i64 %42
  %44 = load i8, i8* %43, align 1, !tbaa !3
  %45 = zext i8 %44 to i32
  %46 = and i32 %40, 7
  %47 = shl nuw nsw i32 1, %46
  %48 = and i32 %47, %45
  %49 = icmp eq i32 %48, 0
  br i1 %49, label %50, label %57

50:                                               ; preds = %37
  %51 = trunc i32 %40 to i8
  %52 = and i8 %51, 7
  %53 = shl nuw i8 1, %52
  %54 = or i8 %53, %44
  store i8 %54, i8* %43, align 1, !tbaa !3
  %55 = add nuw nsw i64 %38, 1
  %56 = icmp eq i64 %55, %36
  br i1 %56, label %58, label %37

57:                                               ; preds = %37
  call void @llvm.lifetime.end.p0i8(i64 32, i8* nonnull %35) #2
  br label %726

58:                                               ; preds = %50
  call void @llvm.lifetime.end.p0i8(i64 32, i8* nonnull %35) #2
  call void @llvm.lifetime.start.p0i8(i64 32, i8* nonnull %35) #2
  call void @llvm.memset.p0i8.i64(i8* nonnull align 16 dereferenceable(32) %35, i8 0, i64 32, i1 false) #2
  br label %59

59:                                               ; preds = %72, %58
  %60 = phi i64 [ 0, %58 ], [ %77, %72 ]
  %61 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %60, i64 1
  %62 = load i32, i32* %61, align 4, !tbaa !22
  %63 = sext i32 %62 to i64
  %64 = lshr i64 %63, 3
  %65 = getelementptr inbounds [32 x i8], [32 x i8]* %6, i64 0, i64 %64
  %66 = load i8, i8* %65, align 1, !tbaa !3
  %67 = zext i8 %66 to i32
  %68 = and i32 %62, 7
  %69 = shl nuw nsw i32 1, %68
  %70 = and i32 %69, %67
  %71 = icmp eq i32 %70, 0
  br i1 %71, label %72, label %79

72:                                               ; preds = %59
  %73 = trunc i32 %62 to i8
  %74 = and i8 %73, 7
  %75 = shl nuw i8 1, %74
  %76 = or i8 %75, %66
  store i8 %76, i8* %65, align 1, !tbaa !3
  %77 = add nuw nsw i64 %60, 1
  %78 = icmp eq i64 %77, %36
  br i1 %78, label %80, label %59

79:                                               ; preds = %59
  call void @llvm.lifetime.end.p0i8(i64 32, i8* nonnull %35) #2
  br label %726

80:                                               ; preds = %72
  call void @llvm.lifetime.end.p0i8(i64 32, i8* nonnull %35) #2
  %81 = mul nsw i32 %2, %2
  %82 = zext i32 %81 to i64
  %83 = tail call i8* @llvm.stacksave()
  %84 = alloca %struct.GF, i64 %82, align 16
  %85 = getelementptr inbounds %struct.GF, %struct.GF* %84, i64 0, i32 0
  call void @llvm.memset.p0i8.i64(i8* nonnull align 16 %85, i8 0, i64 %82, i1 false)
  %86 = icmp sgt i32 %2, 0
  br i1 %86, label %87, label %107

87:                                               ; preds = %80
  %88 = zext i32 %2 to i64
  %89 = zext i32 %2 to i64
  %90 = add nsw i64 %89, -1
  %91 = and i64 %89, 3
  %92 = icmp ult i64 %90, 3
  br i1 %92, label %95, label %93

93:                                               ; preds = %87
  %94 = and i64 %89, 4294967292
  br label %111

95:                                               ; preds = %111, %87
  %96 = phi i64 [ 0, %87 ], [ %129, %111 ]
  %97 = icmp eq i64 %91, 0
  br i1 %97, label %107, label %98

98:                                               ; preds = %95, %98
  %99 = phi i64 [ %104, %98 ], [ %96, %95 ]
  %100 = phi i64 [ %105, %98 ], [ %91, %95 ]
  %101 = mul nsw i64 %99, %88
  %102 = add nuw nsw i64 %101, %99
  %103 = getelementptr inbounds %struct.GF, %struct.GF* %84, i64 %102, i32 0
  store i8 1, i8* %103, align 1, !tbaa !3
  %104 = add nuw nsw i64 %99, 1
  %105 = add i64 %100, -1
  %106 = icmp eq i64 %105, 0
  br i1 %106, label %107, label %98, !llvm.loop !138

107:                                              ; preds = %95, %98, %80
  %108 = zext i32 %4 to i64
  %109 = zext i32 %2 to i64
  %110 = icmp eq i32 %2, 1
  br label %138

111:                                              ; preds = %111, %93
  %112 = phi i64 [ 0, %93 ], [ %129, %111 ]
  %113 = phi i64 [ %94, %93 ], [ %130, %111 ]
  %114 = mul nsw i64 %112, %88
  %115 = add nuw nsw i64 %114, %112
  %116 = getelementptr inbounds %struct.GF, %struct.GF* %84, i64 %115, i32 0
  store i8 1, i8* %116, align 4, !tbaa !3
  %117 = or i64 %112, 1
  %118 = mul nsw i64 %117, %88
  %119 = add nuw nsw i64 %118, %117
  %120 = getelementptr inbounds %struct.GF, %struct.GF* %84, i64 %119, i32 0
  store i8 1, i8* %120, align 1, !tbaa !3
  %121 = or i64 %112, 2
  %122 = mul nsw i64 %121, %88
  %123 = add nuw nsw i64 %122, %121
  %124 = getelementptr inbounds %struct.GF, %struct.GF* %84, i64 %123, i32 0
  store i8 1, i8* %124, align 2, !tbaa !3
  %125 = or i64 %112, 3
  %126 = mul nsw i64 %125, %88
  %127 = add nuw nsw i64 %126, %125
  %128 = getelementptr inbounds %struct.GF, %struct.GF* %84, i64 %127, i32 0
  store i8 1, i8* %128, align 1, !tbaa !3
  %129 = add nuw nsw i64 %112, 4
  %130 = add i64 %113, -4
  %131 = icmp eq i64 %130, 0
  br i1 %131, label %95, label %111

132:                                              ; preds = %159
  call fastcc void @"?mat_inverse@@YAXPEAUGF@@H@Z"(%struct.GF* nonnull %84, i32 %2) #2
  %133 = mul nsw i32 %4, %2
  %134 = zext i32 %133 to i64
  %135 = alloca [2 x <4 x i64>], i64 %134, align 32
  %136 = zext i32 %4 to i64
  %137 = zext i32 %2 to i64
  br label %177

138:                                              ; preds = %159, %107
  %139 = phi i64 [ 0, %107 ], [ %160, %159 ]
  %140 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %139, i64 0
  %141 = load i32, i32* %140, align 4, !tbaa !22
  %142 = mul nsw i32 %141, %2
  %143 = sext i32 %142 to i64
  %144 = getelementptr inbounds %struct.GF, %struct.GF* %84, i64 %143
  %145 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %139, i64 1
  %146 = load i32, i32* %145, align 4, !tbaa !22
  %147 = srem i32 %146, 255
  br i1 %86, label %148, label %159

148:                                              ; preds = %138
  %149 = icmp slt i32 %147, 0
  %150 = add nsw i32 %147, 255
  %151 = select i1 %149, i32 %150, i32 %147
  %152 = add nuw nsw i32 %151, 1
  %153 = zext i32 %152 to i64
  %154 = getelementptr inbounds [256 x i8], [256 x i8]* @"?value_table@GF@@0QBEB", i64 0, i64 %153
  %155 = load i8, i8* %154, align 1, !tbaa !3, !noalias !139
  %156 = zext i8 %155 to i64
  %157 = getelementptr inbounds [256 x i8], [256 x i8]* @"?exp_table@GF@@0QBEB", i64 0, i64 %156
  %158 = getelementptr %struct.GF, %struct.GF* %144, i64 0, i32 0
  store i8 1, i8* %158, align 1, !tbaa !3
  br i1 %110, label %159, label %162

159:                                              ; preds = %148, %162, %138
  %160 = add nuw nsw i64 %139, 1
  %161 = icmp eq i64 %160, %108
  br i1 %161, label %132, label %138

162:                                              ; preds = %148, %162
  %163 = phi i64 [ %175, %162 ], [ 1, %148 ]
  %164 = load i8, i8* %157, align 1, !tbaa !3, !noalias !142
  %165 = zext i8 %164 to i32
  %166 = trunc i64 %163 to i32
  %167 = mul nsw i32 %166, %165
  %168 = urem i32 %167, 255
  %169 = add nuw nsw i32 %168, 1
  %170 = zext i32 %169 to i64
  %171 = getelementptr inbounds [256 x i8], [256 x i8]* @"?value_table@GF@@0QBEB", i64 0, i64 %170
  %172 = load i8, i8* %171, align 1, !tbaa !3, !noalias !145
  %173 = getelementptr inbounds %struct.GF, %struct.GF* %144, i64 %163
  %174 = getelementptr %struct.GF, %struct.GF* %173, i64 0, i32 0
  store i8 %172, i8* %174, align 1, !tbaa !3
  %175 = add nuw nsw i64 %163, 1
  %176 = icmp eq i64 %175, %109
  br i1 %176, label %159, label %162, !llvm.loop !148

177:                                              ; preds = %232, %132
  %178 = phi i64 [ 0, %132 ], [ %234, %232 ]
  %179 = phi i32 [ 0, %132 ], [ %233, %232 ]
  br i1 %86, label %180, label %232

180:                                              ; preds = %177
  %181 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %178, i64 0
  %182 = load i32, i32* %181, align 4, !tbaa !22
  %183 = mul nsw i32 %182, %2
  %184 = sext i32 %179 to i64
  %185 = sext i32 %183 to i64
  br label %219

186:                                              ; preds = %232
  %187 = zext i32 %2 to i64
  %188 = alloca <4 x i64>, i64 %187, align 32
  %189 = icmp slt i32 %16, 32
  br i1 %189, label %506, label %190

190:                                              ; preds = %186
  %191 = add nsw i32 %16, -32
  %192 = bitcast <4 x i64>* %7 to i8*
  %193 = sext i32 %2 to i64
  %194 = sext i32 %191 to i64
  %195 = zext i32 %4 to i64
  %196 = getelementptr inbounds i8, i8* %192, i64 12
  %197 = getelementptr inbounds i8, i8* %192, i64 13
  %198 = getelementptr inbounds i8, i8* %192, i64 14
  %199 = getelementptr inbounds i8, i8* %192, i64 15
  %200 = getelementptr inbounds i8, i8* %192, i64 16
  %201 = getelementptr inbounds i8, i8* %192, i64 17
  %202 = getelementptr inbounds i8, i8* %192, i64 18
  %203 = getelementptr inbounds i8, i8* %192, i64 19
  %204 = getelementptr inbounds i8, i8* %192, i64 20
  %205 = getelementptr inbounds i8, i8* %192, i64 21
  %206 = getelementptr inbounds i8, i8* %192, i64 22
  %207 = getelementptr inbounds i8, i8* %192, i64 23
  %208 = getelementptr inbounds i8, i8* %192, i64 24
  %209 = getelementptr inbounds i8, i8* %192, i64 25
  %210 = getelementptr inbounds i8, i8* %192, i64 26
  %211 = getelementptr inbounds i8, i8* %192, i64 27
  %212 = getelementptr inbounds i8, i8* %192, i64 28
  %213 = getelementptr inbounds i8, i8* %192, i64 29
  %214 = getelementptr inbounds i8, i8* %192, i64 30
  %215 = getelementptr inbounds i8, i8* %192, i64 31
  %216 = icmp ult i32 %2, 16
  %217 = and i64 %109, 4294967280
  %218 = icmp eq i64 %217, %109
  br label %257

219:                                              ; preds = %236, %180
  %220 = phi i64 [ 0, %180 ], [ %237, %236 ]
  %221 = phi i64 [ %184, %180 ], [ %238, %236 ]
  %222 = add nsw i64 %220, %185
  %223 = getelementptr inbounds %struct.GF, %struct.GF* %84, i64 %222, i32 0
  %224 = load i8, i8* %223, align 1, !tbaa.struct !19
  %225 = zext i8 %224 to i64
  %226 = getelementptr inbounds [2 x <4 x i64>], [2 x <4 x i64>]* %135, i64 %221
  %227 = bitcast [2 x <4 x i64>]* %226 to i8*
  %228 = getelementptr inbounds [2 x <4 x i64>], [2 x <4 x i64>]* %135, i64 %221, i64 1
  %229 = bitcast <4 x i64>* %228 to i8*
  br label %240

230:                                              ; preds = %236
  %231 = trunc i64 %238 to i32
  br label %232

232:                                              ; preds = %230, %177
  %233 = phi i32 [ %179, %177 ], [ %231, %230 ]
  %234 = add nuw nsw i64 %178, 1
  %235 = icmp eq i64 %234, %136
  br i1 %235, label %186, label %177

236:                                              ; preds = %240
  %237 = add nuw nsw i64 %220, 1
  %238 = add nsw i64 %221, 1
  %239 = icmp eq i64 %237, %137
  br i1 %239, label %230, label %219

240:                                              ; preds = %240, %219
  %241 = phi i64 [ 0, %219 ], [ %255, %240 ]
  %242 = shl i64 %241, 8
  %243 = or i64 %242, %225
  %244 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %243, i32 0
  %245 = load i8, i8* %244, align 1, !tbaa !3, !noalias !149
  %246 = add nuw nsw i64 %241, 16
  %247 = getelementptr inbounds i8, i8* %227, i64 %246
  store i8 %245, i8* %247, align 1, !tbaa !3
  %248 = getelementptr inbounds i8, i8* %227, i64 %241
  store i8 %245, i8* %248, align 1, !tbaa !3
  %249 = shl nuw nsw i64 %241, 12
  %250 = or i64 %249, %225
  %251 = getelementptr inbounds [65536 x %struct.GF], [65536 x %struct.GF]* @"??@d21f2700854ede5ffe0a07e78d081b25@", i64 0, i64 %250, i32 0
  %252 = load i8, i8* %251, align 1, !tbaa !3, !noalias !152
  %253 = getelementptr inbounds i8, i8* %229, i64 %246
  store i8 %252, i8* %253, align 1, !tbaa !3
  %254 = getelementptr inbounds i8, i8* %229, i64 %241
  store i8 %252, i8* %254, align 1, !tbaa !3
  %255 = add nuw nsw i64 %241, 1
  %256 = icmp eq i64 %255, 16
  br i1 %256, label %236, label %240

257:                                              ; preds = %190, %379
  %258 = phi i64 [ 0, %190 ], [ %380, %379 ]
  %259 = phi i32 [ 0, %190 ], [ %365, %379 ]
  br label %263

260:                                              ; preds = %364
  %261 = mul nsw i64 %258, %193
  %262 = getelementptr inbounds i8, i8* %0, i64 %261
  br label %382

263:                                              ; preds = %364, %257
  %264 = phi i64 [ 0, %257 ], [ %366, %364 ]
  %265 = phi i32 [ %259, %257 ], [ %365, %364 ]
  br i1 %86, label %266, label %364

266:                                              ; preds = %263
  %267 = sext i32 %265 to i64
  br i1 %216, label %268, label %271

268:                                              ; preds = %360, %266
  %269 = phi i64 [ %267, %266 ], [ %272, %360 ]
  %270 = phi i64 [ 0, %266 ], [ %217, %360 ]
  br label %368

271:                                              ; preds = %266
  %272 = add nsw i64 %217, %267
  br label %273

273:                                              ; preds = %273, %271
  %274 = phi i64 [ 0, %271 ], [ %358, %273 ]
  %275 = add i64 %274, %267
  %276 = or i64 %274, 1
  %277 = or i64 %274, 2
  %278 = or i64 %274, 3
  %279 = or i64 %274, 4
  %280 = or i64 %274, 5
  %281 = or i64 %274, 6
  %282 = or i64 %274, 7
  %283 = or i64 %274, 8
  %284 = or i64 %274, 9
  %285 = or i64 %274, 10
  %286 = or i64 %274, 11
  %287 = or i64 %274, 12
  %288 = or i64 %274, 13
  %289 = or i64 %274, 14
  %290 = or i64 %274, 15
  %291 = getelementptr inbounds i8, i8* %0, i64 %275
  %292 = bitcast i8* %291 to <16 x i8>*
  %293 = load <16 x i8>, <16 x i8>* %292, align 1, !tbaa !3
  %294 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %274
  %295 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %276
  %296 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %277
  %297 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %278
  %298 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %279
  %299 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %280
  %300 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %281
  %301 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %282
  %302 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %283
  %303 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %284
  %304 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %285
  %305 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %286
  %306 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %287
  %307 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %288
  %308 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %289
  %309 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %290
  %310 = bitcast <4 x i64>* %294 to i8*
  %311 = bitcast <4 x i64>* %295 to i8*
  %312 = bitcast <4 x i64>* %296 to i8*
  %313 = bitcast <4 x i64>* %297 to i8*
  %314 = bitcast <4 x i64>* %298 to i8*
  %315 = bitcast <4 x i64>* %299 to i8*
  %316 = bitcast <4 x i64>* %300 to i8*
  %317 = bitcast <4 x i64>* %301 to i8*
  %318 = bitcast <4 x i64>* %302 to i8*
  %319 = bitcast <4 x i64>* %303 to i8*
  %320 = bitcast <4 x i64>* %304 to i8*
  %321 = bitcast <4 x i64>* %305 to i8*
  %322 = bitcast <4 x i64>* %306 to i8*
  %323 = bitcast <4 x i64>* %307 to i8*
  %324 = bitcast <4 x i64>* %308 to i8*
  %325 = bitcast <4 x i64>* %309 to i8*
  %326 = getelementptr inbounds i8, i8* %310, i64 %264
  %327 = getelementptr inbounds i8, i8* %311, i64 %264
  %328 = getelementptr inbounds i8, i8* %312, i64 %264
  %329 = getelementptr inbounds i8, i8* %313, i64 %264
  %330 = getelementptr inbounds i8, i8* %314, i64 %264
  %331 = getelementptr inbounds i8, i8* %315, i64 %264
  %332 = getelementptr inbounds i8, i8* %316, i64 %264
  %333 = getelementptr inbounds i8, i8* %317, i64 %264
  %334 = getelementptr inbounds i8, i8* %318, i64 %264
  %335 = getelementptr inbounds i8, i8* %319, i64 %264
  %336 = getelementptr inbounds i8, i8* %320, i64 %264
  %337 = getelementptr inbounds i8, i8* %321, i64 %264
  %338 = getelementptr inbounds i8, i8* %322, i64 %264
  %339 = getelementptr inbounds i8, i8* %323, i64 %264
  %340 = getelementptr inbounds i8, i8* %324, i64 %264
  %341 = getelementptr inbounds i8, i8* %325, i64 %264
  %342 = extractelement <16 x i8> %293, i32 0
  store i8 %342, i8* %326, align 1, !tbaa !3
  %343 = extractelement <16 x i8> %293, i32 1
  store i8 %343, i8* %327, align 1, !tbaa !3
  %344 = extractelement <16 x i8> %293, i32 2
  store i8 %344, i8* %328, align 1, !tbaa !3
  %345 = extractelement <16 x i8> %293, i32 3
  store i8 %345, i8* %329, align 1, !tbaa !3
  %346 = extractelement <16 x i8> %293, i32 4
  store i8 %346, i8* %330, align 1, !tbaa !3
  %347 = extractelement <16 x i8> %293, i32 5
  store i8 %347, i8* %331, align 1, !tbaa !3
  %348 = extractelement <16 x i8> %293, i32 6
  store i8 %348, i8* %332, align 1, !tbaa !3
  %349 = extractelement <16 x i8> %293, i32 7
  store i8 %349, i8* %333, align 1, !tbaa !3
  %350 = extractelement <16 x i8> %293, i32 8
  store i8 %350, i8* %334, align 1, !tbaa !3
  %351 = extractelement <16 x i8> %293, i32 9
  store i8 %351, i8* %335, align 1, !tbaa !3
  %352 = extractelement <16 x i8> %293, i32 10
  store i8 %352, i8* %336, align 1, !tbaa !3
  %353 = extractelement <16 x i8> %293, i32 11
  store i8 %353, i8* %337, align 1, !tbaa !3
  %354 = extractelement <16 x i8> %293, i32 12
  store i8 %354, i8* %338, align 1, !tbaa !3
  %355 = extractelement <16 x i8> %293, i32 13
  store i8 %355, i8* %339, align 1, !tbaa !3
  %356 = extractelement <16 x i8> %293, i32 14
  store i8 %356, i8* %340, align 1, !tbaa !3
  %357 = extractelement <16 x i8> %293, i32 15
  store i8 %357, i8* %341, align 1, !tbaa !3
  %358 = add i64 %274, 16
  %359 = icmp eq i64 %358, %217
  br i1 %359, label %360, label %273, !llvm.loop !155

360:                                              ; preds = %273
  br i1 %218, label %361, label %268

361:                                              ; preds = %368, %360
  %362 = phi i64 [ %272, %360 ], [ %371, %368 ]
  %363 = trunc i64 %362 to i32
  br label %364

364:                                              ; preds = %361, %263
  %365 = phi i32 [ %265, %263 ], [ %363, %361 ]
  %366 = add nuw nsw i64 %264, 1
  %367 = icmp eq i64 %366, 32
  br i1 %367, label %260, label %263

368:                                              ; preds = %268, %368
  %369 = phi i64 [ %371, %368 ], [ %269, %268 ]
  %370 = phi i64 [ %377, %368 ], [ %270, %268 ]
  %371 = add nsw i64 %369, 1
  %372 = getelementptr inbounds i8, i8* %0, i64 %369
  %373 = load i8, i8* %372, align 1, !tbaa !3
  %374 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %370
  %375 = bitcast <4 x i64>* %374 to i8*
  %376 = getelementptr inbounds i8, i8* %375, i64 %264
  store i8 %373, i8* %376, align 1, !tbaa !3
  %377 = add nuw nsw i64 %370, 1
  %378 = icmp eq i64 %377, %187
  br i1 %378, label %361, label %368, !llvm.loop !156

379:                                              ; preds = %410
  %380 = add nuw nsw i64 %258, 32
  %381 = icmp sgt i64 %380, %194
  br i1 %381, label %504, label %257

382:                                              ; preds = %410, %260
  %383 = phi i64 [ 0, %260 ], [ %478, %410 ]
  %384 = phi [2 x <4 x i64>]* [ %135, %260 ], [ %479, %410 ]
  call void @llvm.lifetime.start.p0i8(i64 32, i8* nonnull %192) #2
  store <4 x i64> zeroinitializer, <4 x i64>* %7, align 32, !tbaa !3
  br i1 %86, label %481, label %410

385:                                              ; preds = %481
  %386 = bitcast <4 x i64> %501 to <32 x i8>
  %387 = extractelement <32 x i8> %386, i32 11
  %388 = bitcast <4 x i64> %501 to <32 x i8>
  %389 = extractelement <32 x i8> %388, i32 10
  %390 = bitcast <4 x i64> %501 to <32 x i8>
  %391 = extractelement <32 x i8> %390, i32 9
  %392 = bitcast <4 x i64> %501 to <32 x i8>
  %393 = extractelement <32 x i8> %392, i32 8
  %394 = bitcast <4 x i64> %501 to <32 x i8>
  %395 = extractelement <32 x i8> %394, i32 7
  %396 = bitcast <4 x i64> %501 to <32 x i8>
  %397 = extractelement <32 x i8> %396, i32 6
  %398 = bitcast <4 x i64> %501 to <32 x i8>
  %399 = extractelement <32 x i8> %398, i32 5
  %400 = bitcast <4 x i64> %501 to <32 x i8>
  %401 = extractelement <32 x i8> %400, i32 4
  %402 = bitcast <4 x i64> %501 to <32 x i8>
  %403 = extractelement <32 x i8> %402, i32 3
  %404 = bitcast <4 x i64> %501 to <32 x i8>
  %405 = extractelement <32 x i8> %404, i32 2
  %406 = bitcast <4 x i64> %501 to <32 x i8>
  %407 = extractelement <32 x i8> %406, i32 1
  %408 = bitcast <4 x i64> %501 to <32 x i8>
  %409 = extractelement <32 x i8> %408, i32 0
  br label %410

410:                                              ; preds = %385, %382
  %411 = phi i8 [ %387, %385 ], [ 0, %382 ]
  %412 = phi i8 [ %389, %385 ], [ 0, %382 ]
  %413 = phi i8 [ %391, %385 ], [ 0, %382 ]
  %414 = phi i8 [ %393, %385 ], [ 0, %382 ]
  %415 = phi i8 [ %395, %385 ], [ 0, %382 ]
  %416 = phi i8 [ %397, %385 ], [ 0, %382 ]
  %417 = phi i8 [ %399, %385 ], [ 0, %382 ]
  %418 = phi i8 [ %401, %385 ], [ 0, %382 ]
  %419 = phi i8 [ %403, %385 ], [ 0, %382 ]
  %420 = phi i8 [ %405, %385 ], [ 0, %382 ]
  %421 = phi i8 [ %407, %385 ], [ 0, %382 ]
  %422 = phi i8 [ %409, %385 ], [ 0, %382 ]
  %423 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %383, i64 0
  %424 = load i32, i32* %423, align 4, !tbaa !22
  %425 = sext i32 %424 to i64
  %426 = getelementptr inbounds i8, i8* %262, i64 %425
  store i8 %422, i8* %426, align 1, !tbaa !3
  %427 = getelementptr inbounds i8, i8* %426, i64 %193
  store i8 %421, i8* %427, align 1, !tbaa !3
  %428 = getelementptr inbounds i8, i8* %427, i64 %193
  store i8 %420, i8* %428, align 1, !tbaa !3
  %429 = getelementptr inbounds i8, i8* %428, i64 %193
  store i8 %419, i8* %429, align 1, !tbaa !3
  %430 = getelementptr inbounds i8, i8* %429, i64 %193
  store i8 %418, i8* %430, align 1, !tbaa !3
  %431 = getelementptr inbounds i8, i8* %430, i64 %193
  store i8 %417, i8* %431, align 1, !tbaa !3
  %432 = getelementptr inbounds i8, i8* %431, i64 %193
  store i8 %416, i8* %432, align 1, !tbaa !3
  %433 = getelementptr inbounds i8, i8* %432, i64 %193
  store i8 %415, i8* %433, align 1, !tbaa !3
  %434 = getelementptr inbounds i8, i8* %433, i64 %193
  store i8 %414, i8* %434, align 1, !tbaa !3
  %435 = getelementptr inbounds i8, i8* %434, i64 %193
  store i8 %413, i8* %435, align 1, !tbaa !3
  %436 = getelementptr inbounds i8, i8* %435, i64 %193
  store i8 %412, i8* %436, align 1, !tbaa !3
  %437 = getelementptr inbounds i8, i8* %436, i64 %193
  store i8 %411, i8* %437, align 1, !tbaa !3
  %438 = getelementptr inbounds i8, i8* %437, i64 %193
  %439 = load i8, i8* %196, align 4, !tbaa !3
  store i8 %439, i8* %438, align 1, !tbaa !3
  %440 = getelementptr inbounds i8, i8* %438, i64 %193
  %441 = load i8, i8* %197, align 1, !tbaa !3
  store i8 %441, i8* %440, align 1, !tbaa !3
  %442 = getelementptr inbounds i8, i8* %440, i64 %193
  %443 = load i8, i8* %198, align 2, !tbaa !3
  store i8 %443, i8* %442, align 1, !tbaa !3
  %444 = getelementptr inbounds i8, i8* %442, i64 %193
  %445 = load i8, i8* %199, align 1, !tbaa !3
  store i8 %445, i8* %444, align 1, !tbaa !3
  %446 = getelementptr inbounds i8, i8* %444, i64 %193
  %447 = load i8, i8* %200, align 16, !tbaa !3
  store i8 %447, i8* %446, align 1, !tbaa !3
  %448 = getelementptr inbounds i8, i8* %446, i64 %193
  %449 = load i8, i8* %201, align 1, !tbaa !3
  store i8 %449, i8* %448, align 1, !tbaa !3
  %450 = getelementptr inbounds i8, i8* %448, i64 %193
  %451 = load i8, i8* %202, align 2, !tbaa !3
  store i8 %451, i8* %450, align 1, !tbaa !3
  %452 = getelementptr inbounds i8, i8* %450, i64 %193
  %453 = load i8, i8* %203, align 1, !tbaa !3
  store i8 %453, i8* %452, align 1, !tbaa !3
  %454 = getelementptr inbounds i8, i8* %452, i64 %193
  %455 = load i8, i8* %204, align 4, !tbaa !3
  store i8 %455, i8* %454, align 1, !tbaa !3
  %456 = getelementptr inbounds i8, i8* %454, i64 %193
  %457 = load i8, i8* %205, align 1, !tbaa !3
  store i8 %457, i8* %456, align 1, !tbaa !3
  %458 = getelementptr inbounds i8, i8* %456, i64 %193
  %459 = load i8, i8* %206, align 2, !tbaa !3
  store i8 %459, i8* %458, align 1, !tbaa !3
  %460 = getelementptr inbounds i8, i8* %458, i64 %193
  %461 = load i8, i8* %207, align 1, !tbaa !3
  store i8 %461, i8* %460, align 1, !tbaa !3
  %462 = getelementptr inbounds i8, i8* %460, i64 %193
  %463 = load i8, i8* %208, align 8, !tbaa !3
  store i8 %463, i8* %462, align 1, !tbaa !3
  %464 = getelementptr inbounds i8, i8* %462, i64 %193
  %465 = load i8, i8* %209, align 1, !tbaa !3
  store i8 %465, i8* %464, align 1, !tbaa !3
  %466 = getelementptr inbounds i8, i8* %464, i64 %193
  %467 = load i8, i8* %210, align 2, !tbaa !3
  store i8 %467, i8* %466, align 1, !tbaa !3
  %468 = getelementptr inbounds i8, i8* %466, i64 %193
  %469 = load i8, i8* %211, align 1, !tbaa !3
  store i8 %469, i8* %468, align 1, !tbaa !3
  %470 = getelementptr inbounds i8, i8* %468, i64 %193
  %471 = load i8, i8* %212, align 4, !tbaa !3
  store i8 %471, i8* %470, align 1, !tbaa !3
  %472 = getelementptr inbounds i8, i8* %470, i64 %193
  %473 = load i8, i8* %213, align 1, !tbaa !3
  store i8 %473, i8* %472, align 1, !tbaa !3
  %474 = getelementptr inbounds i8, i8* %472, i64 %193
  %475 = load i8, i8* %214, align 2, !tbaa !3
  store i8 %475, i8* %474, align 1, !tbaa !3
  %476 = getelementptr inbounds i8, i8* %474, i64 %193
  %477 = load i8, i8* %215, align 1, !tbaa !3
  store i8 %477, i8* %476, align 1, !tbaa !3
  call void @llvm.lifetime.end.p0i8(i64 32, i8* nonnull %192) #2
  %478 = add nuw nsw i64 %383, 1
  %479 = getelementptr inbounds [2 x <4 x i64>], [2 x <4 x i64>]* %384, i64 %193
  %480 = icmp eq i64 %478, %195
  br i1 %480, label %379, label %382

481:                                              ; preds = %382, %481
  %482 = phi <4 x i64> [ %501, %481 ], [ zeroinitializer, %382 ]
  %483 = phi i64 [ %502, %481 ], [ 0, %382 ]
  %484 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %483
  %485 = load <4 x i64>, <4 x i64>* %484, align 32, !tbaa !3
  %486 = getelementptr inbounds [2 x <4 x i64>], [2 x <4 x i64>]* %384, i64 %483, i64 0
  %487 = bitcast <4 x i64>* %486 to <32 x i8>*
  %488 = load <32 x i8>, <32 x i8>* %487, align 32, !tbaa !3
  %489 = bitcast <4 x i64> %485 to <32 x i8>
  %490 = and <32 x i8> %489, <i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15>
  %491 = call <32 x i8> @llvm.x86.avx2.pshuf.b(<32 x i8> %488, <32 x i8> %490) #2
  %492 = lshr <4 x i64> %485, <i64 4, i64 4, i64 4, i64 4>
  %493 = getelementptr inbounds [2 x <4 x i64>], [2 x <4 x i64>]* %384, i64 %483, i64 1
  %494 = bitcast <4 x i64>* %493 to <32 x i8>*
  %495 = load <32 x i8>, <32 x i8>* %494, align 32, !tbaa !3
  %496 = bitcast <4 x i64> %492 to <32 x i8>
  %497 = and <32 x i8> %496, <i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15>
  %498 = call <32 x i8> @llvm.x86.avx2.pshuf.b(<32 x i8> %495, <32 x i8> %497) #2
  %499 = xor <32 x i8> %498, %491
  %500 = bitcast <32 x i8> %499 to <4 x i64>
  %501 = xor <4 x i64> %482, %500
  store <4 x i64> %501, <4 x i64>* %7, align 32, !tbaa !3
  %502 = add nuw nsw i64 %483, 1
  %503 = icmp eq i64 %502, %187
  br i1 %503, label %385, label %481

504:                                              ; preds = %379
  %505 = trunc i64 %380 to i32
  br label %506

506:                                              ; preds = %504, %186
  %507 = phi i32 [ 0, %186 ], [ %505, %504 ]
  %508 = phi i32 [ 0, %186 ], [ %365, %504 ]
  %509 = icmp sgt i32 %16, %507
  br i1 %509, label %510, label %725

510:                                              ; preds = %506
  %511 = sub i32 %16, %507
  %512 = icmp sgt i32 %511, 0
  br i1 %512, label %513, label %616

513:                                              ; preds = %510
  %514 = zext i32 %511 to i64
  %515 = icmp ult i32 %2, 16
  %516 = and i64 %109, 4294967280
  %517 = icmp eq i64 %516, %109
  br label %518

518:                                              ; preds = %634, %513
  %519 = phi i64 [ 0, %513 ], [ %636, %634 ]
  %520 = phi i32 [ %508, %513 ], [ %635, %634 ]
  br i1 %86, label %521, label %634

521:                                              ; preds = %518
  %522 = sext i32 %520 to i64
  br i1 %515, label %523, label %526

523:                                              ; preds = %615, %521
  %524 = phi i64 [ %522, %521 ], [ %527, %615 ]
  %525 = phi i64 [ 0, %521 ], [ %516, %615 ]
  br label %638

526:                                              ; preds = %521
  %527 = add nsw i64 %516, %522
  br label %528

528:                                              ; preds = %528, %526
  %529 = phi i64 [ 0, %526 ], [ %613, %528 ]
  %530 = add i64 %529, %522
  %531 = or i64 %529, 1
  %532 = or i64 %529, 2
  %533 = or i64 %529, 3
  %534 = or i64 %529, 4
  %535 = or i64 %529, 5
  %536 = or i64 %529, 6
  %537 = or i64 %529, 7
  %538 = or i64 %529, 8
  %539 = or i64 %529, 9
  %540 = or i64 %529, 10
  %541 = or i64 %529, 11
  %542 = or i64 %529, 12
  %543 = or i64 %529, 13
  %544 = or i64 %529, 14
  %545 = or i64 %529, 15
  %546 = getelementptr inbounds i8, i8* %0, i64 %530
  %547 = bitcast i8* %546 to <16 x i8>*
  %548 = load <16 x i8>, <16 x i8>* %547, align 1, !tbaa !3
  %549 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %529
  %550 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %531
  %551 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %532
  %552 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %533
  %553 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %534
  %554 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %535
  %555 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %536
  %556 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %537
  %557 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %538
  %558 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %539
  %559 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %540
  %560 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %541
  %561 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %542
  %562 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %543
  %563 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %544
  %564 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %545
  %565 = bitcast <4 x i64>* %549 to i8*
  %566 = bitcast <4 x i64>* %550 to i8*
  %567 = bitcast <4 x i64>* %551 to i8*
  %568 = bitcast <4 x i64>* %552 to i8*
  %569 = bitcast <4 x i64>* %553 to i8*
  %570 = bitcast <4 x i64>* %554 to i8*
  %571 = bitcast <4 x i64>* %555 to i8*
  %572 = bitcast <4 x i64>* %556 to i8*
  %573 = bitcast <4 x i64>* %557 to i8*
  %574 = bitcast <4 x i64>* %558 to i8*
  %575 = bitcast <4 x i64>* %559 to i8*
  %576 = bitcast <4 x i64>* %560 to i8*
  %577 = bitcast <4 x i64>* %561 to i8*
  %578 = bitcast <4 x i64>* %562 to i8*
  %579 = bitcast <4 x i64>* %563 to i8*
  %580 = bitcast <4 x i64>* %564 to i8*
  %581 = getelementptr inbounds i8, i8* %565, i64 %519
  %582 = getelementptr inbounds i8, i8* %566, i64 %519
  %583 = getelementptr inbounds i8, i8* %567, i64 %519
  %584 = getelementptr inbounds i8, i8* %568, i64 %519
  %585 = getelementptr inbounds i8, i8* %569, i64 %519
  %586 = getelementptr inbounds i8, i8* %570, i64 %519
  %587 = getelementptr inbounds i8, i8* %571, i64 %519
  %588 = getelementptr inbounds i8, i8* %572, i64 %519
  %589 = getelementptr inbounds i8, i8* %573, i64 %519
  %590 = getelementptr inbounds i8, i8* %574, i64 %519
  %591 = getelementptr inbounds i8, i8* %575, i64 %519
  %592 = getelementptr inbounds i8, i8* %576, i64 %519
  %593 = getelementptr inbounds i8, i8* %577, i64 %519
  %594 = getelementptr inbounds i8, i8* %578, i64 %519
  %595 = getelementptr inbounds i8, i8* %579, i64 %519
  %596 = getelementptr inbounds i8, i8* %580, i64 %519
  %597 = extractelement <16 x i8> %548, i32 0
  store i8 %597, i8* %581, align 1, !tbaa !3
  %598 = extractelement <16 x i8> %548, i32 1
  store i8 %598, i8* %582, align 1, !tbaa !3
  %599 = extractelement <16 x i8> %548, i32 2
  store i8 %599, i8* %583, align 1, !tbaa !3
  %600 = extractelement <16 x i8> %548, i32 3
  store i8 %600, i8* %584, align 1, !tbaa !3
  %601 = extractelement <16 x i8> %548, i32 4
  store i8 %601, i8* %585, align 1, !tbaa !3
  %602 = extractelement <16 x i8> %548, i32 5
  store i8 %602, i8* %586, align 1, !tbaa !3
  %603 = extractelement <16 x i8> %548, i32 6
  store i8 %603, i8* %587, align 1, !tbaa !3
  %604 = extractelement <16 x i8> %548, i32 7
  store i8 %604, i8* %588, align 1, !tbaa !3
  %605 = extractelement <16 x i8> %548, i32 8
  store i8 %605, i8* %589, align 1, !tbaa !3
  %606 = extractelement <16 x i8> %548, i32 9
  store i8 %606, i8* %590, align 1, !tbaa !3
  %607 = extractelement <16 x i8> %548, i32 10
  store i8 %607, i8* %591, align 1, !tbaa !3
  %608 = extractelement <16 x i8> %548, i32 11
  store i8 %608, i8* %592, align 1, !tbaa !3
  %609 = extractelement <16 x i8> %548, i32 12
  store i8 %609, i8* %593, align 1, !tbaa !3
  %610 = extractelement <16 x i8> %548, i32 13
  store i8 %610, i8* %594, align 1, !tbaa !3
  %611 = extractelement <16 x i8> %548, i32 14
  store i8 %611, i8* %595, align 1, !tbaa !3
  %612 = extractelement <16 x i8> %548, i32 15
  store i8 %612, i8* %596, align 1, !tbaa !3
  %613 = add i64 %529, 16
  %614 = icmp eq i64 %613, %516
  br i1 %614, label %615, label %528, !llvm.loop !157

615:                                              ; preds = %528
  br i1 %517, label %631, label %523

616:                                              ; preds = %634, %510
  %617 = bitcast <4 x i64>* %8 to i8*
  %618 = mul nsw i32 %507, %2
  %619 = sext i32 %618 to i64
  %620 = getelementptr inbounds i8, i8* %0, i64 %619
  %621 = sext i32 %2 to i64
  %622 = zext i32 %4 to i64
  %623 = zext i32 %511 to i64
  %624 = add nsw i64 %623, -1
  %625 = add nsw i64 %623, -2
  %626 = icmp eq i32 %511, 1
  %627 = and i64 %624, 3
  %628 = icmp ult i64 %625, 3
  %629 = and i64 %624, -4
  %630 = icmp eq i64 %627, 0
  br label %649

631:                                              ; preds = %638, %615
  %632 = phi i64 [ %527, %615 ], [ %641, %638 ]
  %633 = trunc i64 %632 to i32
  br label %634

634:                                              ; preds = %631, %518
  %635 = phi i32 [ %520, %518 ], [ %633, %631 ]
  %636 = add nuw nsw i64 %519, 1
  %637 = icmp eq i64 %636, %514
  br i1 %637, label %616, label %518

638:                                              ; preds = %523, %638
  %639 = phi i64 [ %641, %638 ], [ %524, %523 ]
  %640 = phi i64 [ %647, %638 ], [ %525, %523 ]
  %641 = add nsw i64 %639, 1
  %642 = getelementptr inbounds i8, i8* %0, i64 %639
  %643 = load i8, i8* %642, align 1, !tbaa !3
  %644 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %640
  %645 = bitcast <4 x i64>* %644 to i8*
  %646 = getelementptr inbounds i8, i8* %645, i64 %519
  store i8 %643, i8* %646, align 1, !tbaa !3
  %647 = add nuw nsw i64 %640, 1
  %648 = icmp eq i64 %647, %187
  br i1 %648, label %631, label %638, !llvm.loop !158

649:                                              ; preds = %699, %616
  %650 = phi i64 [ 0, %616 ], [ %700, %699 ]
  %651 = phi [2 x <4 x i64>]* [ %135, %616 ], [ %701, %699 ]
  call void @llvm.lifetime.start.p0i8(i64 32, i8* nonnull %617) #2
  store <4 x i64> zeroinitializer, <4 x i64>* %8, align 32, !tbaa !3
  br i1 %86, label %663, label %655

652:                                              ; preds = %663
  %653 = bitcast <4 x i64> %683 to <32 x i8>
  %654 = extractelement <32 x i8> %653, i32 0
  br label %655

655:                                              ; preds = %652, %649
  %656 = phi i8 [ %654, %652 ], [ 0, %649 ]
  br i1 %512, label %657, label %699

657:                                              ; preds = %655
  %658 = getelementptr inbounds [2 x i32], [2 x i32]* %3, i64 %650, i64 0
  %659 = load i32, i32* %658, align 4, !tbaa !22
  %660 = sext i32 %659 to i64
  %661 = getelementptr inbounds i8, i8* %620, i64 %660
  store i8 %656, i8* %661, align 1, !tbaa !3
  br i1 %626, label %699, label %662

662:                                              ; preds = %657
  br i1 %628, label %686, label %703

663:                                              ; preds = %649, %663
  %664 = phi <4 x i64> [ %683, %663 ], [ zeroinitializer, %649 ]
  %665 = phi i64 [ %684, %663 ], [ 0, %649 ]
  %666 = getelementptr inbounds <4 x i64>, <4 x i64>* %188, i64 %665
  %667 = load <4 x i64>, <4 x i64>* %666, align 32, !tbaa !3
  %668 = getelementptr inbounds [2 x <4 x i64>], [2 x <4 x i64>]* %651, i64 %665, i64 0
  %669 = bitcast <4 x i64>* %668 to <32 x i8>*
  %670 = load <32 x i8>, <32 x i8>* %669, align 32, !tbaa !3
  %671 = bitcast <4 x i64> %667 to <32 x i8>
  %672 = and <32 x i8> %671, <i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15>
  %673 = call <32 x i8> @llvm.x86.avx2.pshuf.b(<32 x i8> %670, <32 x i8> %672) #2
  %674 = lshr <4 x i64> %667, <i64 4, i64 4, i64 4, i64 4>
  %675 = getelementptr inbounds [2 x <4 x i64>], [2 x <4 x i64>]* %651, i64 %665, i64 1
  %676 = bitcast <4 x i64>* %675 to <32 x i8>*
  %677 = load <32 x i8>, <32 x i8>* %676, align 32, !tbaa !3
  %678 = bitcast <4 x i64> %674 to <32 x i8>
  %679 = and <32 x i8> %678, <i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15, i8 15>
  %680 = call <32 x i8> @llvm.x86.avx2.pshuf.b(<32 x i8> %677, <32 x i8> %679) #2
  %681 = xor <32 x i8> %680, %673
  %682 = bitcast <32 x i8> %681 to <4 x i64>
  %683 = xor <4 x i64> %664, %682
  store <4 x i64> %683, <4 x i64>* %8, align 32, !tbaa !3
  %684 = add nuw nsw i64 %665, 1
  %685 = icmp eq i64 %684, %187
  br i1 %685, label %652, label %663

686:                                              ; preds = %703, %662
  %687 = phi i64 [ 1, %662 ], [ %722, %703 ]
  %688 = phi i8* [ %661, %662 ], [ %719, %703 ]
  br i1 %630, label %699, label %689

689:                                              ; preds = %686, %689
  %690 = phi i64 [ %696, %689 ], [ %687, %686 ]
  %691 = phi i8* [ %693, %689 ], [ %688, %686 ]
  %692 = phi i64 [ %697, %689 ], [ %627, %686 ]
  %693 = getelementptr inbounds i8, i8* %691, i64 %621
  %694 = getelementptr inbounds i8, i8* %617, i64 %690
  %695 = load i8, i8* %694, align 1, !tbaa !3
  store i8 %695, i8* %693, align 1, !tbaa !3
  %696 = add nuw nsw i64 %690, 1
  %697 = add i64 %692, -1
  %698 = icmp eq i64 %697, 0
  br i1 %698, label %699, label %689, !llvm.loop !159

699:                                              ; preds = %686, %689, %657, %655
  call void @llvm.lifetime.end.p0i8(i64 32, i8* nonnull %617) #2
  %700 = add nuw nsw i64 %650, 1
  %701 = getelementptr inbounds [2 x <4 x i64>], [2 x <4 x i64>]* %651, i64 %621
  %702 = icmp eq i64 %700, %622
  br i1 %702, label %725, label %649

703:                                              ; preds = %662, %703
  %704 = phi i64 [ %722, %703 ], [ 1, %662 ]
  %705 = phi i8* [ %719, %703 ], [ %661, %662 ]
  %706 = phi i64 [ %723, %703 ], [ %629, %662 ]
  %707 = getelementptr inbounds i8, i8* %705, i64 %621
  %708 = getelementptr inbounds i8, i8* %617, i64 %704
  %709 = load i8, i8* %708, align 1, !tbaa !3
  store i8 %709, i8* %707, align 1, !tbaa !3
  %710 = add nuw nsw i64 %704, 1
  %711 = getelementptr inbounds i8, i8* %707, i64 %621
  %712 = getelementptr inbounds i8, i8* %617, i64 %710
  %713 = load i8, i8* %712, align 1, !tbaa !3
  store i8 %713, i8* %711, align 1, !tbaa !3
  %714 = add nuw nsw i64 %704, 2
  %715 = getelementptr inbounds i8, i8* %711, i64 %621
  %716 = getelementptr inbounds i8, i8* %617, i64 %714
  %717 = load i8, i8* %716, align 1, !tbaa !3
  store i8 %717, i8* %715, align 1, !tbaa !3
  %718 = add nuw nsw i64 %704, 3
  %719 = getelementptr inbounds i8, i8* %715, i64 %621
  %720 = getelementptr inbounds i8, i8* %617, i64 %718
  %721 = load i8, i8* %720, align 1, !tbaa !3
  store i8 %721, i8* %719, align 1, !tbaa !3
  %722 = add nuw nsw i64 %704, 4
  %723 = add i64 %706, -4
  %724 = icmp eq i64 %723, 0
  br i1 %724, label %686, label %703

725:                                              ; preds = %699, %506
  call void @llvm.stackrestore(i8* %83)
  br label %726

726:                                              ; preds = %22, %29, %79, %57, %14, %11, %5, %725
  %727 = phi i32 [ 0, %725 ], [ 1, %5 ], [ 2, %11 ], [ 3, %14 ], [ 4, %79 ], [ 4, %57 ], [ 4, %29 ], [ 4, %22 ]
  ret i32 %727
}

; Function Attrs: nounwind readnone
declare <16 x i8> @llvm.x86.ssse3.pshuf.b.128(<16 x i8>, <16 x i8>) #6

; Function Attrs: nounwind readnone
declare <32 x i8> @llvm.x86.avx2.pshuf.b(<32 x i8>, <32 x i8>) #6

; Function Attrs: argmemonly nounwind willreturn writeonly
declare void @llvm.memset.p0i8.i64(i8* nocapture writeonly, i8, i64, i1 immarg) #7

declare dso_local i32 @__CxxFrameHandler3(...)

attributes #0 = { nounwind uwtable "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "min-legal-vector-width"="0" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+avx,+avx2,+cx8,+fxsr,+mmx,+popcnt,+sse,+sse2,+sse3,+sse4.1,+sse4.2,+ssse3,+x87,+xsave" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #1 = { argmemonly nounwind willreturn }
attributes #2 = { nounwind }
attributes #3 = { inaccessiblemem_or_argmemonly nounwind willreturn }
attributes #4 = { nounwind uwtable "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "min-legal-vector-width"="128" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+avx,+avx2,+cx8,+fxsr,+mmx,+popcnt,+sse,+sse2,+sse3,+sse4.1,+sse4.2,+ssse3,+x87,+xsave" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #5 = { nounwind uwtable "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "min-legal-vector-width"="256" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+avx,+avx2,+cx8,+fxsr,+mmx,+popcnt,+sse,+sse2,+sse3,+sse4.1,+sse4.2,+ssse3,+x87,+xsave" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #6 = { nounwind readnone }
attributes #7 = { argmemonly nounwind willreturn writeonly }

!llvm.module.flags = !{!0, !1}
!llvm.ident = !{!2}

!0 = !{i32 1, !"wchar_size", i32 2}
!1 = !{i32 7, !"PIC Level", i32 2}
!2 = !{!"clang version 11.0.0"}
!3 = !{!4, !4, i64 0}
!4 = !{!"omnipotent char", !5, i64 0}
!5 = !{!"Simple C++ TBAA"}
!6 = !{!7}
!7 = distinct !{!7, !8, !"?from_exp@GF@@SA?AU1@H@Z: argument 0"}
!8 = distinct !{!8, !"?from_exp@GF@@SA?AU1@H@Z"}
!9 = !{!10, !10, i64 0}
!10 = !{!"any pointer", !4, i64 0}
!11 = !{!12}
!12 = distinct !{!12, !13, !"?pow@GF@@QEBA?AU1@H@Z: argument 0"}
!13 = distinct !{!13, !"?pow@GF@@QEBA?AU1@H@Z"}
!14 = !{!15, !12}
!15 = distinct !{!15, !16, !"?from_exp@GF@@SA?AU1@H@Z: argument 0"}
!16 = distinct !{!16, !"?from_exp@GF@@SA?AU1@H@Z"}
!17 = distinct !{!17, !18}
!18 = !{!"llvm.loop.peeled.count", i32 1}
!19 = !{i64 0, i64 1, !3}
!20 = distinct !{!20, !21}
!21 = !{!"llvm.loop.unroll.disable"}
!22 = !{!23, !23, i64 0}
!23 = !{!"int", !4, i64 0}
!24 = distinct !{!24, !21}
!25 = !{!26}
!26 = distinct !{!26, !27, !"?from_exp@GF@@SA?AU1@H@Z: argument 0"}
!27 = distinct !{!27, !"?from_exp@GF@@SA?AU1@H@Z"}
!28 = !{!29}
!29 = distinct !{!29, !30, !"?pow@GF@@QEBA?AU1@H@Z: argument 0"}
!30 = distinct !{!30, !"?pow@GF@@QEBA?AU1@H@Z"}
!31 = !{!32, !29}
!32 = distinct !{!32, !33, !"?from_exp@GF@@SA?AU1@H@Z: argument 0"}
!33 = distinct !{!33, !"?from_exp@GF@@SA?AU1@H@Z"}
!34 = distinct !{!34, !18}
!35 = !{!36, !4, i64 0}
!36 = !{!"?AUGF@@", !4, i64 0}
!37 = distinct !{!37, !21}
!38 = distinct !{!38, !21}
!39 = distinct !{!39, !21}
!40 = !{!41, !43}
!41 = distinct !{!41, !42, !"?inverse@GF@@QEBA?AU1@XZ: argument 0"}
!42 = distinct !{!42, !"?inverse@GF@@QEBA?AU1@XZ"}
!43 = distinct !{!43, !44, !"??K@YA?AUGF@@U0@0@Z: argument 0"}
!44 = distinct !{!44, !"??K@YA?AUGF@@U0@0@Z"}
!45 = !{!46, !43}
!46 = distinct !{!46, !47, !"??D@YA?AUGF@@U0@0@Z: argument 0"}
!47 = distinct !{!47, !"??D@YA?AUGF@@U0@0@Z"}
!48 = !{!49}
!49 = distinct !{!49, !50, !"??D@YA?AUGF@@U0@0@Z: argument 0"}
!50 = distinct !{!50, !"??D@YA?AUGF@@U0@0@Z"}
!51 = !{!52}
!52 = distinct !{!52, !53, !"??D@YA?AUGF@@U0@0@Z: argument 0"}
!53 = distinct !{!53, !"??D@YA?AUGF@@U0@0@Z"}
!54 = !{!55, !57}
!55 = distinct !{!55, !56, !"?inverse@GF@@QEBA?AU1@XZ: argument 0"}
!56 = distinct !{!56, !"?inverse@GF@@QEBA?AU1@XZ"}
!57 = distinct !{!57, !58, !"??K@YA?AUGF@@U0@0@Z: argument 0"}
!58 = distinct !{!58, !"??K@YA?AUGF@@U0@0@Z"}
!59 = !{!60, !57}
!60 = distinct !{!60, !61, !"??D@YA?AUGF@@U0@0@Z: argument 0"}
!61 = distinct !{!61, !"??D@YA?AUGF@@U0@0@Z"}
!62 = !{!63}
!63 = distinct !{!63, !64, !"??D@YA?AUGF@@U0@0@Z: argument 0"}
!64 = distinct !{!64, !"??D@YA?AUGF@@U0@0@Z"}
!65 = !{!66}
!66 = distinct !{!66, !67, !"??D@YA?AUGF@@U0@0@Z: argument 0"}
!67 = distinct !{!67, !"??D@YA?AUGF@@U0@0@Z"}
!68 = !{!69}
!69 = distinct !{!69, !70, !"?inverse@GF@@QEBA?AU1@XZ: argument 0"}
!70 = distinct !{!70, !"?inverse@GF@@QEBA?AU1@XZ"}
!71 = !{!72}
!72 = distinct !{!72, !73, !"??D@YA?AUGF@@U0@0@Z: argument 0"}
!73 = distinct !{!73, !"??D@YA?AUGF@@U0@0@Z"}
!74 = !{!75}
!75 = distinct !{!75, !76, !"?from_exp@GF@@SA?AU1@H@Z: argument 0"}
!76 = distinct !{!76, !"?from_exp@GF@@SA?AU1@H@Z"}
!77 = !{!78}
!78 = distinct !{!78, !79, !"?pow@GF@@QEBA?AU1@H@Z: argument 0"}
!79 = distinct !{!79, !"?pow@GF@@QEBA?AU1@H@Z"}
!80 = !{!81, !78}
!81 = distinct !{!81, !82, !"?from_exp@GF@@SA?AU1@H@Z: argument 0"}
!82 = distinct !{!82, !"?from_exp@GF@@SA?AU1@H@Z"}
!83 = !{!84}
!84 = distinct !{!84, !85, !"??D@YA?AUGF@@U0@0@Z: argument 0"}
!85 = distinct !{!85, !"??D@YA?AUGF@@U0@0@Z"}
!86 = !{!87}
!87 = distinct !{!87, !88, !"??D@YA?AUGF@@U0@0@Z: argument 0"}
!88 = distinct !{!88, !"??D@YA?AUGF@@U0@0@Z"}
!89 = distinct !{!89, !90}
!90 = !{!"llvm.loop.isvectorized", i32 1}
!91 = distinct !{!91, !92, !90}
!92 = !{!"llvm.loop.unroll.runtime.disable"}
!93 = distinct !{!93, !90}
!94 = distinct !{!94, !92, !90}
!95 = distinct !{!95, !21}
!96 = distinct !{!96, !21}
!97 = !{!98}
!98 = distinct !{!98, !99, !"?from_exp@GF@@SA?AU1@H@Z: argument 0"}
!99 = distinct !{!99, !"?from_exp@GF@@SA?AU1@H@Z"}
!100 = !{!101}
!101 = distinct !{!101, !102, !"?pow@GF@@QEBA?AU1@H@Z: argument 0"}
!102 = distinct !{!102, !"?pow@GF@@QEBA?AU1@H@Z"}
!103 = !{!104, !101}
!104 = distinct !{!104, !105, !"?from_exp@GF@@SA?AU1@H@Z: argument 0"}
!105 = distinct !{!105, !"?from_exp@GF@@SA?AU1@H@Z"}
!106 = distinct !{!106, !18}
!107 = !{!108}
!108 = distinct !{!108, !109, !"??D@YA?AUGF@@U0@0@Z: argument 0"}
!109 = distinct !{!109, !"??D@YA?AUGF@@U0@0@Z"}
!110 = !{!111}
!111 = distinct !{!111, !112, !"??D@YA?AUGF@@U0@0@Z: argument 0"}
!112 = distinct !{!112, !"??D@YA?AUGF@@U0@0@Z"}
!113 = distinct !{!113, !90}
!114 = distinct !{!114, !92, !90}
!115 = distinct !{!115, !90}
!116 = distinct !{!116, !92, !90}
!117 = distinct !{!117, !21}
!118 = !{!119}
!119 = distinct !{!119, !120, !"?from_exp@GF@@SA?AU1@H@Z: argument 0"}
!120 = distinct !{!120, !"?from_exp@GF@@SA?AU1@H@Z"}
!121 = !{!122}
!122 = distinct !{!122, !123, !"?pow@GF@@QEBA?AU1@H@Z: argument 0"}
!123 = distinct !{!123, !"?pow@GF@@QEBA?AU1@H@Z"}
!124 = !{!125, !122}
!125 = distinct !{!125, !126, !"?from_exp@GF@@SA?AU1@H@Z: argument 0"}
!126 = distinct !{!126, !"?from_exp@GF@@SA?AU1@H@Z"}
!127 = !{!128}
!128 = distinct !{!128, !129, !"??D@YA?AUGF@@U0@0@Z: argument 0"}
!129 = distinct !{!129, !"??D@YA?AUGF@@U0@0@Z"}
!130 = !{!131}
!131 = distinct !{!131, !132, !"??D@YA?AUGF@@U0@0@Z: argument 0"}
!132 = distinct !{!132, !"??D@YA?AUGF@@U0@0@Z"}
!133 = distinct !{!133, !90}
!134 = distinct !{!134, !92, !90}
!135 = distinct !{!135, !90}
!136 = distinct !{!136, !92, !90}
!137 = distinct !{!137, !21}
!138 = distinct !{!138, !21}
!139 = !{!140}
!140 = distinct !{!140, !141, !"?from_exp@GF@@SA?AU1@H@Z: argument 0"}
!141 = distinct !{!141, !"?from_exp@GF@@SA?AU1@H@Z"}
!142 = !{!143}
!143 = distinct !{!143, !144, !"?pow@GF@@QEBA?AU1@H@Z: argument 0"}
!144 = distinct !{!144, !"?pow@GF@@QEBA?AU1@H@Z"}
!145 = !{!146, !143}
!146 = distinct !{!146, !147, !"?from_exp@GF@@SA?AU1@H@Z: argument 0"}
!147 = distinct !{!147, !"?from_exp@GF@@SA?AU1@H@Z"}
!148 = distinct !{!148, !18}
!149 = !{!150}
!150 = distinct !{!150, !151, !"??D@YA?AUGF@@U0@0@Z: argument 0"}
!151 = distinct !{!151, !"??D@YA?AUGF@@U0@0@Z"}
!152 = !{!153}
!153 = distinct !{!153, !154, !"??D@YA?AUGF@@U0@0@Z: argument 0"}
!154 = distinct !{!154, !"??D@YA?AUGF@@U0@0@Z"}
!155 = distinct !{!155, !90}
!156 = distinct !{!156, !92, !90}
!157 = distinct !{!157, !90}
!158 = distinct !{!158, !92, !90}
!159 = distinct !{!159, !21}