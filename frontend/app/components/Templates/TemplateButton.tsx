import React, { useRef } from 'react';
import { StyleSheet, Text, View, Pressable, DeviceEventEmitter, Animated } from 'react-native';
import { Template } from '@/app/types/Template';
import { useApi } from '@/app/context/ApiContext';
import { useGlobalButtons } from './ButtonProvider';
import { useTestUser } from '@/app/context/TestUserContext';

interface TemplateButtonProps {
    template: Template,
    setActiveTemplate: (template: Template) => void
    setModalVisible: (visibility: boolean) => void
}

export interface ExpensePayload {
    name: string,
    group: string,
    category: string,
    tag: string,
    amount: number,
    description: string
}

export default function TemplateButton({ template, setActiveTemplate, setModalVisible }: TemplateButtonProps) {
    const { activeSpreadsheetId } = useTestUser()

    const api = useApi()
    const { isAnyButtonBusy, triggerAction } = useGlobalButtons();

    const scaleValue = useRef(new Animated.Value(1)).current;

    const handlePressIn = () => {
        if (isAnyButtonBusy) return;
        Animated.spring(scaleValue, {
        toValue: 0.95, // Clean 5% shrink
        tension: 300,  // Fast initial press down
        friction: 20,
        useNativeDriver: true,
        }).start();
    };

    const handlePressOut = () => {
        Animated.spring(scaleValue, {
        toValue: 1,    // Crisp spring back
        tension: 250,
        friction: 12,  // Slight elastic recoil at the end
        useNativeDriver: true,
        }).start();
    };

    return (
        <Animated.View style={{ transform: [{ scale: scaleValue }] }}>
            <Pressable
                onPress={() => {
                    setActiveTemplate(template)
                    setModalVisible(true)
                }}
                onPressIn={handlePressIn}
                onPressOut={handlePressOut}
                unstable_pressDelay={120} // 120ms delay gives the ScrollView time to claim the gesture
                pressRetentionOffset={{ top: 10, left: 10, right: 10, bottom: 10 }}
                disabled={isAnyButtonBusy}
                style={styles.section}
            >
                <Text style={styles.groupText}>
                    {template.group}
                </Text>

                <View style={styles.topRow}>
                    <Text style={styles.topRowText}>
                        {template.name}
                    </Text>
                    <Text style={styles.topRowText}>
                        ₱{template.amount}
                    </Text>
                </View>

                <Text style={styles.categoryText}>
                    {template.category}
                </Text>
                <Text style={styles.tagText}>
                    {template.tag}
                </Text>

                <Text style={[
                    template.description.trim() ? styles.descriptionText: styles.noDescriptionText,
                    {marginTop: 12}
                ]}>
                    {template.description.trim() ? template.description : 'No description'}
                </Text>
            </Pressable>
        </Animated.View>
    )
}

const styles = StyleSheet.create({
    section: {
        paddingHorizontal: 20,
        paddingVertical: 16,
        backgroundColor: 'white',

        borderRadius: 20,
        borderWidth: 0.1,
        borderColor: 'gray'
    },
    topRow: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        marginBottom: 4
    },
    topRowText: {
        fontWeight: 700,
        fontSize: 18
    },
    groupText: {
        fontWeight: 800,
        fontSize: 12,
        color: 'gray'
    },
    categoryText: {
        fontWeight: 500,
        fontSize: 12,
        color: 'gray'
    },
    tagText: {
        fontWeight: 500,
        fontSize: 12,
        color: 'gray'
    },
    descriptionText: {
        fontWeight: 500,
        fontSize: 12,
        color: 'gray'
    },
    noDescriptionText: {
        fontWeight: 300,
        fontSize: 12,
        color: 'gray'
    }
})