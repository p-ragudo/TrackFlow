import React, { useEffect } from "react";
import { Pressable, StyleSheet, Text, View } from "react-native";
import FloatingOptionButton from "./FloatingOptionButton";
import Animated, {
    Easing,
    useAnimatedStyle,
    useSharedValue,
    withTiming,
} from "react-native-reanimated";

interface AddButtonProps {
    isOpen: boolean
    onToggle: () => void
}

export default function AddButton({ isOpen, onToggle }: AddButtonProps) {
    const animationProgress = useSharedValue(0);

    useEffect(() => {
        // Fast, snappy duration with easeOut for opening and easeIn for closing
        animationProgress.value = withTiming(isOpen ? 1 : 0, {
            duration: 180, // Fast 180ms transition
            easing: isOpen ? Easing.out(Easing.quad) : Easing.in(Easing.quad),
        });
    }, [isOpen]);

    const optionsAnimatedStyle = useAnimatedStyle(() => ({
        opacity: animationProgress.value,
        transform: [
            // Pure vertical slide down/up with no bounce or scale overshoot
            { translateY: (1 - animationProgress.value) * 12 },
        ],
        pointerEvents: animationProgress.value === 0 ? ("none" as const) : ("auto" as const),
    }));

    // Rotate + by 45deg to create X
    const iconAnimatedStyle = useAnimatedStyle(() => ({
        transform: [
            {
                rotate: `${animationProgress.value * 45}deg`,
            },
        ],
    }));

    const handleOptionPress = (optionName: string) => {
        console.log(`${optionName} pressed!`)
    }

    return (
        <View style={styles.floatingContainer}>
            <Animated.View style={[styles.optionsWrapper, optionsAnimatedStyle]}>
                <FloatingOptionButton 
                    text="Add custom expense" 
                    onPress={() => handleOptionPress("Add custom expense")} 
                />
                <FloatingOptionButton 
                    text="Add Template" 
                    onPress={() => handleOptionPress("Add Template")} 
                />
            </Animated.View>

            {/* Main FAB Trigger */}
            <Pressable 
                style={styles.fab}
                onPress={onToggle}
            >
                <Animated.View style={iconAnimatedStyle}>
                    <Text style={styles.plusText}>+</Text>
                </Animated.View>
            </Pressable>
        </View>
    )
}

const styles = StyleSheet.create({
    floatingContainer: {
        position: 'absolute',
        bottom: 20,
        alignSelf: 'center',
        alignItems: 'center',
        zIndex: 20, // Above overlay backdrop
    },
    optionsWrapper: {
        alignItems: 'center',
        marginBottom: 12,
        gap: 3,
    },
    fab: {
        width: 60,
        height: 60,
        borderRadius: 30,
        backgroundColor: '#000000',
        alignItems: 'center',
        justifyContent: 'center',
        elevation: 6,
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 3 },
        shadowOpacity: 0.3,
        shadowRadius: 4,
    },
    plusText: {
        color: '#FFFFFF',
        fontSize: 32,
        fontWeight: '300',
        lineHeight: 34,
        textAlign: 'center',
    },
})